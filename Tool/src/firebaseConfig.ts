export const firebaseConfig = {
  apiKey: 'AIzaSyDv3j_mWYMEGGRvxKdx-qmixS64Wd8cfxM',
  authDomain: 'spt-ammogen-modpatches.firebaseapp.com',
  databaseURL: 'https://spt-ammogen-modpatches-default-rtdb.europe-west1.firebasedatabase.app',
  projectId: 'spt-ammogen-modpatches',
  storageBucket: 'spt-ammogen-modpatches.firebasestorage.app',
  messagingSenderId: '790563023555',
  appId: '1:790563023555:web:2f949d207af0a05d85d076',
  measurementId: 'G-S8DHWHYC5C',
}

export const firebaseEnabled = Object.values(firebaseConfig).every(v => v && String(v).trim() !== '')
