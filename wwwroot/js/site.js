document.addEventListener('DOMContentLoaded',()=>{
 document.querySelectorAll('[data-open-service-modal]').forEach(button=>button.addEventListener('click',()=>bootstrap.Modal.getOrCreateInstance(document.getElementById('serviceRequestModal')).show()));
 const select=document.getElementById('heroVehicleSelect');if(!select)return;
 const samples={polo:['Volkswagen Polo 1.0 TSI','JT 42 ZN',25],hilux:['Toyota Hilux 2.4 GD-6','LM 81 TK',145],bmw:['BMW 320i','KP 19 FX',6]};
 function update(){const [name,plate,days]=samples[select.value];const expiry=new Date();expiry.setDate(expiry.getDate()+days);const values={'.js-vehicle-model':name,'.js-vehicle-plate':plate,'.js-renewal-days':String(days),'.js-renewal-date':expiry.toLocaleDateString('en-ZA',{day:'numeric',month:'long',year:'numeric'}),'.js-renewal-status':days<=7?'Urgent renewal':days<=30?'Due soon':'Up to date','.js-renewal-message':'Illustrative preview. Open the portal to manage your own vehicles.'};for(const [selector,text] of Object.entries(values))document.querySelectorAll(selector).forEach(el=>el.textContent=text);document.querySelector('.q4u-floating-icon-yellow').textContent=String(days);}
 select.addEventListener('change',update);update();
});
