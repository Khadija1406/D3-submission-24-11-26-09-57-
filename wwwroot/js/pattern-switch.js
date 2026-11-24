
(function(){
 const button=document.getElementById('pattern-switch');
 if(!button)return;
 let alternate=false;
 button.addEventListener('click',function(){
  alternate=!alternate;
  const title=document.getElementById('pattern-result-title');
  const copy=document.getElementById('pattern-result-copy');
  const list=document.getElementById('pattern-questions');
  if(alternate){
   title.textContent='A few small delays may share a pattern.';
   copy.textContent='Imagine reporting missed deadlines, a cluttered task list, and repeatedly forgetting small errands. Pulse could help you explore whether planning overload is showing up in several places.';
   list.innerHTML='<li>Are several tasks competing for your attention?</li><li>Do unfinished tasks tend to carry into the next day?</li><li>Would breaking one task into smaller steps help?</li>';
   button.innerHTML='Back to the first example <span aria-hidden="true">↗</span>';
  }else{
   title.textContent='These things might be connected.';
   copy.textContent='Your reports suggest a possible relationship between your sleep routine, energy levels, and concentration.';
   list.innerHTML='<li>Has your sleep schedule changed recently?</li><li>Do you notice lower concentration after poor sleep?</li><li>Does your energy improve when your routine changes?</li>';
   button.innerHTML='Explore another example <span aria-hidden="true">↗</span>';
  }
 });
})();
