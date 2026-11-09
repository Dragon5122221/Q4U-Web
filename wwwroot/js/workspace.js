document.addEventListener('DOMContentLoaded',()=>{
const menu=document.getElementById('menu-toggle'),nav=document.getElementById('workspace-nav');
menu?.addEventListener('click',()=>{const open=nav.classList.toggle('open');menu.setAttribute('aria-expanded',String(open));if(open)nav.querySelector('a')?.focus();});
document.addEventListener('keydown',e=>{if(e.key==='Escape'&&nav.classList.contains('open')){nav.classList.remove('open');menu.setAttribute('aria-expanded','false');menu.focus();}});
document.getElementById('form-error')?.focus();
// Confirmation uses a native modal dialog: keyboard focus stays inside it and Escape cancels.
const dialog=document.getElementById('confirmation');let pending=null;
document.querySelectorAll('form[data-confirm]').forEach(form=>form.addEventListener('submit',event=>{if(form.dataset.confirmed==='yes')return;event.preventDefault();pending=form;document.getElementById('confirmation-message').textContent=form.dataset.confirm;dialog.returnValue='cancel';dialog.showModal();dialog.querySelector('[value=cancel]').focus();}));
dialog.addEventListener('close',()=>{if(dialog.returnValue==='confirm'&&pending){pending.dataset.confirmed='yes';pending.requestSubmit();}pending=null;});
// Draft text remains in this tab only. Never store tokens, document bytes or passwords.
const draftPrefix='q4u.draft.';
if(document.querySelector('.feedback.success')){try{Object.keys(sessionStorage).filter(k=>k.startsWith(draftPrefix)).forEach(k=>sessionStorage.removeItem(k));}catch{}}
document.querySelectorAll('.draft-form').forEach((form,i)=>{const key=draftPrefix+location.pathname+new URLSearchParams(location.search).get('section')+'.'+i;const fields=[...form.querySelectorAll('input:not([type=hidden]),textarea,select')];try{const saved=JSON.parse(sessionStorage.getItem(key)||'{}');for(const f of fields)if(saved[f.name]!==undefined)f.value=saved[f.name];}catch{}form.addEventListener('input',()=>{try{sessionStorage.setItem(key,JSON.stringify(Object.fromEntries(fields.map(f=>[f.name,f.value]))));}catch{}});});
document.querySelector('form[action="/Account/Logout"]')?.addEventListener('submit',()=>{try{Object.keys(sessionStorage).filter(k=>k.startsWith(draftPrefix)).forEach(k=>sessionStorage.removeItem(k));}catch{}});
// Accessible help tips work with pointer, focus, touch and Escape.
let tip=null,tipSource=null;
function closeTip(){tip?.remove();tip=null;if(tipSource){tipSource.removeAttribute('aria-describedby');tipSource.setAttribute('aria-expanded','false');}tipSource=null;}
document.querySelectorAll('[data-help]').forEach((button,i)=>{button.setAttribute('aria-expanded','false');const open=()=>{closeTip();tipSource=button;tip=document.createElement('div');tip.className='help-popup';tip.id='help-tip-'+i;tip.setAttribute('role','tooltip');tip.textContent=button.dataset.help;document.body.append(tip);const r=button.getBoundingClientRect();tip.style.left=Math.max(8,Math.min(r.left,innerWidth-tip.offsetWidth-8))+'px';tip.style.top=Math.max(8,Math.min(r.bottom+10,innerHeight-tip.offsetHeight-8))+'px';button.setAttribute('aria-describedby',tip.id);button.setAttribute('aria-expanded','true');};button.addEventListener('click',open);button.addEventListener('focus',open);button.addEventListener('mouseenter',open);button.addEventListener('blur',closeTip);button.addEventListener('mouseleave',()=>{setTimeout(()=>{if(tip&&!tip.matches(':hover')&&document.activeElement!==button)closeTip();},200);});});
document.addEventListener('keydown',e=>{if(e.key==='Escape')closeTip();});window.addEventListener('resize',closeTip);document.addEventListener('pointerdown',e=>{if(tip && e.target!==tipSource && !tip.contains(e.target))closeTip();});
document.querySelectorAll('[data-filter]').forEach(input=>input.addEventListener('input',()=>{const rows=[...document.getElementsByClassName(input.dataset.filter)];const term=input.value.toLowerCase().trim();rows.forEach(row=>row.hidden=!row.textContent.toLowerCase().includes(term));const empty=input.closest('.panel').querySelector('.filter-empty');if(empty)empty.hidden=rows.some(row=>!row.hidden);}));
function online(){document.getElementById('connection-status').textContent=navigator.onLine?'':'You are offline. Reconnect before saving.';}window.addEventListener('online',online);window.addEventListener('offline',online);online();
if(location.hash){const target=document.getElementById(location.hash.slice(1));if(target?.tagName==='DETAILS')target.open=true;target?.scrollIntoView();}
});
