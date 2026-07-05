// Copy this file to firebaseConfig.ts and fill in your Firebase project credentials.
// The real firebaseConfig.ts is gitignored so credentials are never committed.

export const firebaseConfig = {
  apiKey: '',
  authDomain: '',
  databaseURL: '',
  projectId: '',
  storageBucket: '',
  messagingSenderId: '',
  appId: '',
  measurementId: '',
}

export const firebaseEnabled = Object.values(firebaseConfig).every(v => v && String(v).trim() !== '')
