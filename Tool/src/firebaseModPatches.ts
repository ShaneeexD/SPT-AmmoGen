import { initializeApp } from 'firebase/app'
import { getAuth, signInAnonymously } from 'firebase/auth'
import {
  getDatabase,
  ref,
  onValue,
  set,
  push,
  remove,
  Unsubscribe,
  DataSnapshot,
} from 'firebase/database'
import { firebaseConfig, firebaseEnabled } from './firebaseConfig'
import type { ModFilterPatch } from './types'

const app = firebaseEnabled ? initializeApp(firebaseConfig) : null
const db = app ? getDatabase(app) : null
const auth = app ? getAuth(app) : null

let authPromise: Promise<void> | null = null

function ensureFirebaseAuth(): Promise<void> {
  if (!firebaseEnabled || !auth) return Promise.resolve()
  if (auth.currentUser) return Promise.resolve()
  if (authPromise) return authPromise
  authPromise = signInAnonymously(auth)
    .then(() => undefined)
    .catch(err => {
      authPromise = null
      throw err
    })
  return authPromise
}

const DB_PATH = 'modFilterPatches'

export interface ModPatchWithKey extends ModFilterPatch {
  key: string
}

export function isFirebaseEnabled(): boolean {
  return firebaseEnabled && db !== null
}

export function subscribeToModPatches(
  onChange: (patches: ModPatchWithKey[]) => void,
  onError: (err: Error) => void
): Unsubscribe | null {
  if (!db) return null

  let unsubscribe: Unsubscribe | null = null

  ensureFirebaseAuth()
    .then(() => {
      const patchesRef = ref(db, DB_PATH)
      unsubscribe = onValue(
        patchesRef,
        snapshot => {
          onChange(snapshotToPatches(snapshot))
        },
        err => {
          console.error('[AmmoGen] Firebase mod patches subscription failed:', err)
          onError(err)
        }
      )
    })
    .catch(err => {
      console.error('[AmmoGen] Firebase auth failed:', err)
      onError(err)
    })

  return () => unsubscribe?.()
}

export async function addModPatch(patch: ModFilterPatch): Promise<string> {
  if (!db) throw new Error('Firebase is not enabled')
  await ensureFirebaseAuth()
  const newRef = push(ref(db, DB_PATH))
  await set(newRef, patchToValue(patch))
  return newRef.key!
}

export async function updateModPatch(key: string, patch: ModFilterPatch): Promise<void> {
  if (!db) throw new Error('Firebase is not enabled')
  await ensureFirebaseAuth()
  await set(ref(db, `${DB_PATH}/${key}`), patchToValue(patch))
}

export async function removeModPatch(key: string): Promise<void> {
  if (!db) throw new Error('Firebase is not enabled')
  await ensureFirebaseAuth()
  await remove(ref(db, `${DB_PATH}/${key}`))
}

export async function setAllModPatches(patches: ModPatchWithKey[]): Promise<void> {
  if (!db) throw new Error('Firebase is not enabled')
  await ensureFirebaseAuth()
  const value: Record<string, ReturnType<typeof patchToValue>> = {}
  patches.forEach(({ key, ...patch }) => {
    value[key] = patchToValue(patch)
  })
  await set(ref(db, DB_PATH), value)
}

function patchToValue(patch: ModFilterPatch): Record<string, unknown> {
  return {
    guid: patch.guid,
    name: patch.name,
    ammoIds: patch.ammoIds,
    weaponIds: patch.weaponIds,
    magazineIds: patch.magazineIds,
  }
}

function snapshotToPatches(snapshot: DataSnapshot): ModPatchWithKey[] {
  const raw = snapshot.val()
  if (!raw || typeof raw !== 'object') return []
  return Object.entries(raw).map(([key, data]) => ({
    key,
    guid: (data as any).guid ?? '',
    name: (data as any).name ?? '',
    ammoIds: (data as any).ammoIds ?? [],
    weaponIds: (data as any).weaponIds ?? [],
    magazineIds: (data as any).magazineIds ?? [],
  }))
}
