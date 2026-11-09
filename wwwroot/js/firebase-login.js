const form=document.getElementById('firebase-login');
if(form){
const status=document.getElementById('auth-status');
try {
 const [{initializeApp},{getAuth,signInWithEmailAndPassword,sendPasswordResetEmail,setPersistence,inMemoryPersistence,signOut}]=await Promise.all([import('https://www.gstatic.com/firebasejs/12.3.0/firebase-app.js'),import('https://www.gstatic.com/firebasejs/12.3.0/firebase-auth.js')]);
 const auth=getAuth(initializeApp({apiKey:form.dataset.apiKey,authDomain:form.dataset.authDomain,projectId:form.dataset.project}));
 await setPersistence(auth,inMemoryPersistence);
 form.addEventListener('submit',async event=>{event.preventDefault();const button=form.querySelector('[type=submit]');button.disabled=true;status.textContent='Signing in…';try{const result=await signInWithEmailAndPassword(auth,document.getElementById('firebase-email').value,document.getElementById('firebase-password').value);document.getElementById('firebase-token').value=await result.user.getIdToken();await signOut(auth);document.getElementById('firebase-exchange').submit();}catch{status.textContent='Sign-in failed. Check your email and password, or contact Q4U.';button.disabled=false;}});
 document.getElementById('firebase-reset').addEventListener('click',async()=>{const email=document.getElementById('firebase-email');if(!email.value || !email.checkValidity()){email.reportValidity();return;}try{await sendPasswordResetEmail(auth,email.value);}catch{}status.textContent='If that account can receive a reset email, instructions will be sent. Check your inbox and spam folder.';});
}catch{status.textContent='Authentication could not load. Check the Firebase configuration and your connection.';form.querySelector('[type=submit]').disabled=true;}
}
