(function(){
 const KEY='pulse.problems.v1';
 const CATS={
  technical:{n:'Technical',e:'💻',d:'Internet, software & devices',c:'oklch(0.75 0.08 225)'},
  personal:{n:'Personal',e:'🌷',d:'Everyday personal challenges',c:'oklch(0.75 0.11 10)'},
  managerial:{n:'Managerial',e:'🗂️',d:'Planning & coordination',c:'oklch(0.72 0.1 300)'},
  academic:{n:'Academic',e:'📚',d:'Study & assignments',c:'oklch(0.82 0.11 85)'},
  household:{n:'Household',e:'🏠',d:'Home & appliances',c:'oklch(0.76 0.1 140)'},
  lifestyle:{n:'Lifestyle',e:'☕',d:'Routine & time',c:'oklch(0.8 0.09 50)'}
 };
 const STATUS={active:'Active',progress:'In progress',resolved:'Resolved'};
 const $=(s,r=document)=>r.querySelector(s), $$=(s,r=document)=>[...r.querySelectorAll(s)];
 const esc=s=>String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
 const iso=d=>{const x=new Date(d);x.setMinutes(x.getMinutes()-x.getTimezoneOffset());return x.toISOString().slice(0,10)};
 const ago=n=>iso(Date.now()-n*864e5);
 const fmt=s=>new Date(s+'T00:00').toLocaleDateString(undefined,{day:'numeric',month:'short',year:'numeric'});
 const uid=()=>Math.random().toString(36).slice(2,9);

 function seed(){return [
  {id:uid(),title:'Wi-Fi keeps dropping in the evening',desc:'The connection disconnects around 8–10pm, mostly during video calls. Restarting the router helps for a while.',cat:'technical',status:'active',impact:'High',created:ago(20),occ:[{d:ago(20),n:'First noticed during a class call.'},{d:ago(12),n:'Dropped twice in one hour.'},{d:ago(6),n:'Restarted router, back after 5 min.'},{d:ago(2),n:'Dropped during a meeting again.'},{d:ago(0),n:'Slow and disconnecting.'}],tried:{}},
  {id:uid(),title:'Missing assignment deadlines',desc:'I keep losing track of due dates when several courses have work due in the same week.',cat:'academic',status:'progress',impact:'Medium',created:ago(15),occ:[{d:ago(15),n:'Late on the database report.'},{d:ago(8),n:'Nearly missed the quiz.'},{d:ago(3),n:'Submitted lab 2 hours late.'}],tried:{}},
  {id:uid(),title:'Kitchen tap is leaking',desc:'Slow drip from the kitchen tap, gets worse at night.',cat:'household',status:'resolved',impact:'Low',created:ago(25),occ:[{d:ago(25),n:'Noticed the drip.'},{d:ago(18),n:'Replaced washer — fixed.'}],tried:{}},
  {id:uid(),title:'Sleeping too late on weekdays',desc:'Going to bed after 2am, which makes mornings hard and lowers my focus.',cat:'lifestyle',status:'active',impact:'Medium',created:ago(10),occ:[{d:ago(10),n:''},{d:ago(5),n:'Phone scrolling until late.'},{d:ago(1),n:''}],tried:{}},
  {id:uid(),title:'Team tasks overlapping',desc:'Two people end up working on the same task because nobody updates the board.',cat:'managerial',status:'active',impact:'Medium',created:ago(4),occ:[{d:ago(4),n:'Duplicate slides made.'}],tried:{}}
 ]}
 let data; try{data=JSON.parse(localStorage.getItem(KEY))}catch(e){} if(!Array.isArray(data)){data=seed();save()}
 function save(){localStorage.setItem(KEY,JSON.stringify(data))}
 
 // Asynchronous Database Sync with ASP.NET Core & ADO.NET
 async function syncWithServer(){
  try{
   const res = await fetch('/Problems/GetAll');
   if(res.ok){
    const serverData = await res.json();
    if(Array.isArray(serverData) && serverData.length > 0){
     data = serverData;
     save();
     const h = location.hash.slice(1).toLowerCase();
     const isDetail = h.startsWith('problem/');
     if(APP.includes(h) || isDetail){
      render(isDetail ? 'detail' : h, isDetail ? location.hash.slice(9) : null);
     }
    }
   }
  }catch(e){
   // Offline or local cache fallback
  }
  try{
   const ures = await fetch('/Account/CurrentUser');
   if(ures.ok){
    const udata = await ures.json();
    if(udata.isAuthenticated && udata.user && udata.user.name){
     let uName = udata.user.name;
     if(!uName.includes('@')){
      localStorage.setItem('pulse.user', uName);
      userBits();
     }
    }
   }
  }catch(e){}
 }
 window.PulseApp = { sync: syncWithServer };

 const last=p=>p.occ.reduce((a,o)=>o.d>a?o.d:a,p.created);
 const isAlert=p=>p.occ.length>3&&p.status!=='resolved';

 function toast(t){const el=$('#pa-toast');el.textContent=t;el.classList.add('show');clearTimeout(toast.t);toast.t=setTimeout(()=>el.classList.remove('show'),2600)}

 /* ---------- routing ---------- */
 const APP=['dashboard','add-problem','problems','alerts'];
 function route(){
  const h=location.hash.slice(1).toLowerCase();
  const isDetail=h.startsWith('problem/');
  const isApp=APP.includes(h)||isDetail;
  document.body.classList.toggle('app-open',isApp);
  if(!isApp)return;
  $('main').style.display='none';
  $$('.auth-view').forEach(v=>v.classList.remove('is-active'));
  document.body.classList.remove('auth-open');
  const view=isDetail?'detail':h;
  $$('.pa-view').forEach(v=>v.classList.toggle('on',v.dataset.view===view));
  $$('[data-nav]').forEach(a=>a.classList.toggle('on',a.dataset.nav===view||(isDetail&&a.dataset.nav==='problems')));
  render(view,isDetail?location.hash.slice(9):null);
  window.scrollTo(0,0);
 }
 window.addEventListener('hashchange',route);

 $('[data-logout]').addEventListener('click',()=>localStorage.removeItem('pulse.user'));

 function userBits(){
  let n = (window.__INITIAL_USER__ && window.__INITIAL_USER__ !== 'Friend') 
      ? window.__INITIAL_USER__ 
      : (localStorage.getItem('pulse.user') || 'Friend');

  // If someone had an old stripped email with numbers stored in localStorage, clean it up
  if(n.includes('@') || /^[a-zA-Z]+\d+$/.test(n)){
   if(window.__INITIAL_USER__ && !window.__INITIAL_USER__.includes('@')){
    n = window.__INITIAL_USER__;
   } else {
    n = 'Friend';
   }
   localStorage.setItem('pulse.user', n);
  }

  $$('[data-user-name], #greeting-name').forEach(e => e.textContent = n);
  $$('[data-user-initial]').forEach(e => e.textContent = n.charAt(0).toUpperCase());
  $$('[data-today]').forEach(e => e.textContent = new Date().toLocaleDateString(undefined,{weekday:'long',day:'numeric',month:'short'}));
  $$('[data-alert-count]').forEach(e => {
   const count = data.filter(isAlert).length;
   e.textContent = count;
   e.style.display = count ? 'inline-block' : 'none';
  });
 }

 const row=p=>`<a class="pa-row" href="#problem/${p.id}"><span class="pa-cat-ic c-${p.cat}">${CATS[p.cat].e}</span><div><div class="t">${esc(p.title)}</div><div class="m">${CATS[p.cat].n} · last on ${fmt(last(p))}</div></div><div class="r">${isAlert(p)?'<span class="pill pill-alert">↻ Recurring</span>':''}<span class="pill st-${p.status}">${STATUS[p.status]}</span></div></a>`;
 const alertBox=p=>`<div class="pa-alert"><span class="dot">🔔</span><div><b>${esc(p.title)}</b>This has happened <b style="display:inline">${p.occ.length} times</b> — more than three. It may be worth looking at the pattern. <a href="#problem/${p.id}">View history →</a></div></div>`;

 function render(view,id){
  userBits();
  if(view==='dashboard')dash();
  if(view==='problems')list();
  if(view==='alerts'){const a=data.filter(isAlert);$('[data-alerts-full]').innerHTML=a.length?a.map(alertBox).join(''):'<div class="pa-empty">✿ No recurring problems right now. Nice and calm.</div>'}
  if(view==='detail')detail(id);
  if(view==='add-problem')resetForm(false);
 }

 function dash(){
  const all=data.flatMap(p=>p.occ);
  $('[data-stat=total]').textContent=data.length;
  $('[data-stat=active]').textContent=data.filter(p=>p.status!=='resolved').length;
  $('[data-stat=resolved]').textContent=data.filter(p=>p.status==='resolved').length;
  $('[data-stat=occ]').textContent=all.length;
  const days=[...Array(7)].map((_,i)=>ago(6-i));const counts=days.map(d=>all.filter(o=>o.d===d).length);const mx=Math.max(1,...counts);
  $('[data-week-bars]').innerHTML=counts.map(c=>`<div class="pa-bar"><small>${c}</small><i style="height:0%" data-h="${c/mx*85}"></i></div>`).join('');
  $('[data-week-labels]').innerHTML=days.map(d=>`<span>${new Date(d+'T00:00').toLocaleDateString(undefined,{weekday:'short'})}</span>`).join('');
  requestAnimationFrame(()=>$$('[data-h]').forEach(b=>b.style.height=b.dataset.h+'%'));
  const byCat=Object.keys(CATS).map(k=>[k,data.filter(p=>p.cat===k).length]).filter(x=>x[1]);
  let acc=0;const tot=data.length||1;
  $('[data-donut]').style.background=byCat.length?`conic-gradient(${byCat.map(([k,v])=>{const s=acc;acc+=v/tot*360;return `${CATS[k].c} ${s}deg ${acc}deg`}).join(',')})`:'var(--p-line)';
  $('[data-donut-total]').textContent=data.length;
  $('[data-legend]').innerHTML=byCat.map(([k,v])=>`<li><i style="background:${CATS[k].c}"></i>${CATS[k].n}<b style="margin-left:auto;padding-left:10px">${v}</b></li>`).join('')||'<li>No problems yet</li>';
  const rec=[...data].sort((a,b)=>last(b).localeCompare(last(a))).slice(0,4);
  $('[data-recent]').innerHTML=rec.length?rec.map(row).join(''):'<div class="pa-empty">Nothing here yet. <a href="#add-problem">Add your first problem</a>.</div>';
  const al=data.filter(isAlert);
  $('[data-dash-alerts]').innerHTML=al.length?al.map(alertBox).join(''):'<div class="pa-empty">✿ No recurring problems. Lovely.</div>';
 }

 /* ---------- list ---------- */
 const F={q:'',cat:'all',status:'all',sort:'recent'};
 $('[data-filter-cat]').innerHTML=['all',...Object.keys(CATS)].map(k=>`<button data-v="${k}" class="${k==='all'?'on':''}">${k==='all'?'All':CATS[k].e+' '+CATS[k].n}</button>`).join('');
 $('[data-filter-status]').innerHTML=['all','active','progress','resolved'].map(k=>`<button data-v="${k}" class="${k==='all'?'on':''}">${k==='all'?'Any status':STATUS[k]}</button>`).join('');
 [['[data-filter-cat]','cat'],['[data-filter-status]','status']].forEach(([s,k])=>$(s).addEventListener('click',e=>{const b=e.target.closest('button');if(!b)return;F[k]=b.dataset.v;$$('button',$(s)).forEach(x=>x.classList.toggle('on',x===b));list()}));
 $('[data-search]').addEventListener('input',e=>{F.q=e.target.value.toLowerCase();list()});
 $('[data-sort]').addEventListener('change',e=>{F.sort=e.target.value;list()});
 function list(){
  let r=data.filter(p=>(F.cat==='all'||p.cat===F.cat)&&(F.status==='all'||p.status===F.status)&&(p.title+' '+p.desc).toLowerCase().includes(F.q));
  r.sort(F.sort==='most'?(a,b)=>b.occ.length-a.occ.length:F.sort==='az'?(a,b)=>a.title.localeCompare(b.title):(a,b)=>last(b).localeCompare(last(a)));
  $('[data-plist]').innerHTML=r.length?r.map(p=>`<a class="pa-card pa-pcard" href="#problem/${p.id}"><div style="display:flex;gap:6px;align-items:center"><span class="pill c-${p.cat}">${CATS[p.cat].e} ${CATS[p.cat].n}</span><span class="pill st-${p.status}" style="margin-left:auto">${STATUS[p.status]}</span></div><h3>${esc(p.title)}</h3><p>${esc(p.desc)}</p><div><div class="pa-hint" style="margin:0 0 6px"><span>Occurrences</span><span>${p.occ.length} / 4 alert</span></div><div class="pa-meter"><i style="width:${Math.min(100,p.occ.length/4*100)}%"></i></div></div><div class="foot">${isAlert(p)?'<span class="pill pill-alert">↻ Recurring</span>':''}<span class="pill pill-count">Impact: ${p.impact}</span><span style="margin-left:auto;font-size:11.5px;color:var(--p-faint)">Last: ${fmt(last(p))}</span></div></a>`).join(''):'<div class="pa-card pa-empty" style="grid-column:1/-1">No problems match these filters.</div>';
 }

 /* ---------- assistant suggestions (rule-based demo; AI to be connected later) ---------- */
 const TIPS={
  technical:['Note the exact time each issue happens — patterns often point to network congestion or scheduled updates.','Restart and update the affected device or router firmware, then log whether it recurs.','Test with a different device or cable to see whether the problem is the device or the connection.'],
  personal:['Write down what was happening just before each occurrence to spot triggers.','Try one small change for a week and record how it feels.','Talk it through with someone you trust — saying it aloud often clarifies it.'],
  managerial:['Assign one clear owner to each task and make the board the single source of truth.','Hold a 10-minute check-in at the start of the week.','Break large tasks into smaller items with their own due dates.'],
  academic:['Put every due date into one calendar with reminders two days before.','Start each assignment with a 25-minute session the day it is set.','Ask your instructor early if deadlines cluster in the same week.'],
  household:['Check whether a worn part (washer, filter, fuse) is the likely cause.','Record when the problem gets worse — time of day often hints at pressure or usage.','If it repeats after a DIY fix, book a professional and share your history.'],
  lifestyle:['Set a fixed wind-down alarm 45 minutes before bed.','Keep your phone out of reach for the last half hour of the day.','Track one small habit daily and review the week on Sunday.']
 };

 function detail(id){
  const p=data.find(x=>x.id===id);const box=$('[data-detail]');
  if(!p){box.innerHTML='<div class="pa-card pa-empty">This problem could not be found. <a href="#problems">Back to list</a></div>';return}
  const occ=p.occ.map((o,i)=>({...o,i})).sort((a,b)=>b.d.localeCompare(a.d)||b.i-a.i);
  const pattern=p.occ.length>1?(()=>{const s=[...p.occ].map(o=>+new Date(o.d)).sort();const gap=Math.max(1,Math.round((s[s.length-1]-s[0])/864e5/(s.length-1)));return `On average every <b>${gap} day${gap===1?'':'s'}</b>`})():'Only one occurrence so far';
  box.innerHTML=`
  <div class="pa-top"><div><div style="display:flex;gap:6px;margin-bottom:10px"><span class="pill c-${p.cat}">${CATS[p.cat].e} ${CATS[p.cat].n}</span><span class="pill st-${p.status}">${STATUS[p.status]}</span>${isAlert(p)?'<span class="pill pill-alert">↻ Recurring</span>':''}</div><h1>${esc(p.title)}</h1><p style="max-width:640px">${esc(p.desc)}</p></div>
   <div style="display:flex;gap:8px;flex-wrap:wrap"><select class="pa-input" style="width:auto;background:var(--p-paper)" data-set-status>${Object.keys(STATUS).map(k=>`<option value="${k}" ${k===p.status?'selected':''}>${STATUS[k]}</option>`).join('')}</select><button class="pa-btn pa-btn-ghost" data-del>Delete</button></div></div>
  ${isAlert(p)?alertBox(p).replace(/<a href[^>]*>.*?<\/a>/,''):''}
  <div class="pa-grid">
   <div class="pa-card"><h3>Occurrence history</h3><p class="sub">Each time this happened, in order.</p>
    <form class="pa-occ-form" data-occ-form><input type="date" class="pa-input" style="flex:0 0 170px" value="${iso(Date.now())}" max="${iso(Date.now())}" required/><input class="pa-input" placeholder="Optional note — what happened this time?" maxlength="160"/><button class="pa-btn pa-btn-dark">＋ Log occurrence</button></form>
    <ul class="pa-timeline">${occ.map((o,i)=>`<li class="${i===occ.length-1?'first':''}"><b>Occurrence #${p.occ.length-i}${i===occ.length-1?' · first reported':''}</b><small>${fmt(o.d)}</small>${o.n?`<p>${esc(o.n)}</p>`:''}</li>`).join('')}</ul>
   </div>
   <div style="display:grid;gap:18px;align-content:start">
    <div class="pa-card"><h3>At a glance</h3><p class="sub">Summary of this record.</p><div class="pa-meta"><div><span>Occurrences</span><b>${p.occ.length}</b></div><div><span>Impact</span><b>${p.impact}</b></div><div><span>First reported</span><b>${fmt(p.created)}</b></div><div><span>Last seen</span><b>${fmt(last(p))}</b></div><div style="grid-column:1/-1"><span>Pattern</span><b style="font-weight:500">${pattern}</b></div></div></div>
    <div class="pa-card pa-ai"><h3>✦ Assistant suggestions</h3><p class="sub">Based on the category and ${p.occ.length} recorded occurrence${p.occ.length===1?'':'s'}. Mark what you tried.</p>
     ${TIPS[p.cat].map((t,i)=>{const s=p.tried[i];return `<div class="pa-sugg"><span class="n">${i+1}</span><div>${t}<div class="acts"><button data-try="${i}" data-r="yes" class="${s==='yes'?'on':''}">✓ Helped</button><button data-try="${i}" data-r="no" class="no ${s==='no'?'on':''}">✕ Didn't help</button></div></div></div>`}).join('')}
    </div>
   </div>
  </div>`;
  $('[data-occ-form]',box).addEventListener('submit',e=>{e.preventDefault();const [d,n]=$$('input',e.target);if(!d.value||d.value>iso(Date.now())){toast('Pick a date that is not in the future');return}
   const before=isAlert(p);p.occ.push({d:d.value,n:n.value.trim()});if(p.status==='resolved')p.status='active';save();detail(id);userBits();
   fetch('/Problems/AddOccurrence', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ problemId: p.id, date: d.value, note: n.value.trim() })
   }).catch(e=>{});
   toast(!before&&isAlert(p)?'🔔 Recurring alert: this happened more than 3 times':'Occurrence logged ♡')});
  $('[data-set-status]',box).addEventListener('change',e=>{p.status=e.target.value;save();detail(id);userBits();toast('Status updated to '+STATUS[p.status]);
   fetch('/Problems/UpdateStatus', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id: p.id, status: p.status })
   }).catch(e=>{});});
  $('[data-del]',box).addEventListener('click',()=>{if(!confirm('Delete this problem and its history?'))return;data=data.filter(x=>x!==p);save();toast('Problem deleted');location.hash='#problems';
   fetch('/Problems/Delete?id='+encodeURIComponent(p.id), { method: 'POST' }).catch(e=>{});});
  $$('[data-try]',box).forEach(b=>b.addEventListener('click',()=>{const i=b.dataset.try;p.tried[i]=p.tried[i]===b.dataset.r?undefined:b.dataset.r;save();detail(id)}));
 }

 /* ---------- add form ---------- */
 const form=$('#pa-add-form');const st={cat:'',status:'active',impact:'Medium'};
 $('[data-cats]').innerHTML=Object.entries(CATS).map(([k,c])=>`<button type="button" class="pa-cat" data-cat="${k}"><span class="e">${c.e}</span><b>${c.n}</b><small>${c.d}</small></button>`).join('');
 $('[data-cats]').addEventListener('click',e=>{const b=e.target.closest('.pa-cat');if(!b)return;st.cat=b.dataset.cat;$$('.pa-cat').forEach(x=>x.classList.toggle('on',x===b));$('[data-f=cat]').classList.remove('bad');pv()});
 $$('[data-seg]').forEach(g=>g.addEventListener('click',e=>{const b=e.target.closest('button');if(!b)return;st[g.dataset.seg]=b.dataset.v;$$('button',g).forEach(x=>x.classList.toggle('on',x===b));pv()}));
 const T=$('#pa-title'),D=$('#pa-desc'),DT=$('#pa-date');
 [T,D].forEach(i=>i.addEventListener('input',()=>{i.closest('.pa-field').classList.remove('bad');pv()}));
 function pv(){
  $('[data-count-title]').textContent=T.value.length+'/80';$('[data-count-desc]').textContent=D.value.length+'/600';
  $('[data-pv-title]').textContent=T.value||'Your problem title';$('[data-pv-desc]').textContent=D.value||'Your description will appear here.';
  const c=$('[data-pv-cat]');c.className='pill c-'+(st.cat||'technical');c.textContent=st.cat?CATS[st.cat].e+' '+CATS[st.cat].n:'Category';
  const s=$('[data-pv-status]');s.className='pill st-'+st.status;s.textContent=STATUS[st.status];
 }
 function resetForm(clear){
  DT.max=iso(Date.now());if(clear||!DT.value)DT.value=iso(Date.now());
  if(clear){T.value='';D.value='';st.cat='';st.status='active';st.impact='Medium';$$('.pa-cat').forEach(x=>x.classList.remove('on'));
   $$('[data-seg] button').forEach(b=>b.classList.toggle('on',b.dataset.v===st.status||b.dataset.v===st.impact));$$('.pa-field').forEach(f=>f.classList.remove('bad'))}
  pv();
 }
 form.addEventListener('reset',e=>{e.preventDefault();resetForm(true)});
 form.addEventListener('submit',e=>{e.preventDefault();
  const bad={title:T.value.trim().length<3,desc:D.value.trim().length<10,cat:!st.cat,date:!DT.value||DT.value>iso(Date.now())};
  Object.entries(bad).forEach(([k,v])=>$(`[data-f=${k}]`).classList.toggle('bad',v));
  if(Object.values(bad).some(Boolean)){toast('A few details still need a look');return}
  const p={id:uid(),title:T.value.trim(),desc:D.value.trim(),cat:st.cat,status:st.status,impact:st.impact,created:DT.value,occ:[{d:DT.value,n:'First reported.'}],tried:{}};
  data.unshift(p);save();resetForm(true);toast('Problem saved ✨');location.hash='#problem/'+p.id;
  fetch('/Problems/Create', {
   method: 'POST',
   headers: { 'Content-Type': 'application/json' },
   body: JSON.stringify({ title: p.title, desc: p.desc, cat: p.cat, status: p.status, impact: p.impact, created: p.created })
  }).then(r=>r.json()).then(res=>{if(res&&res.id){p.id=res.id;save();}}).catch(e=>{});
 });

 syncWithServer();
 route();
})();
