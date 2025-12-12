async function loadAll(){
  const r = await fetch('/words');
  const data = await r.json();
  showResults(data);
}
async function doSearch(){
  const q = document.getElementById('search').value;
  const r = await fetch('/search?q=' + encodeURIComponent(q));
  const data = await r.json();
  showResults(data);
}
function showResults(items){
  const el = document.getElementById('results');
  if(!items || items.length === 0){ el.innerHTML = '<i>No results</i>'; return; }
  el.innerHTML = items.map(it => `
    <div class="item">
      <b>${escapeHtml(it.term)}</b>
      <p>${escapeHtml(it.definition)}</p>
      <small>tags: ${(it.tags || []).map(escapeHtml).join(', ')}</small>
      <div><button onclick="deleteWord('${encodeURIComponent(it.term)}')">Delete</button></div>
    </div>
  `).join('');
}
function escapeHtml(s){ return (s||'').replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c])); }
async function addWord(){
  const term = document.getElementById('term').value.trim();
  const definition = document.getElementById('definition').value.trim();
  const tags = document.getElementById('tags').value.split(',').map(s=>s.trim()).filter(Boolean);
  const r = await fetch('/word', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({term,definition,tags}) });
  if(r.status === 201) { alert('Added'); loadAll(); } else { const e = await r.json(); alert('Error: '+(e.error||JSON.stringify(e))); }
}
async function updateWord(){
  const term = document.getElementById('term').value.trim();
  const definition = document.getElementById('definition').value.trim();
  const tags = document.getElementById('tags').value.split(',').map(s=>s.trim()).filter(Boolean);
  const r = await fetch('/word/' + encodeURIComponent(term), { method:'PUT', headers:{'Content-Type':'application/json'}, body: JSON.stringify({definition,tags}) });
  if(r.ok) { alert('Updated'); loadAll(); } else { const e = await r.json(); alert('Error: '+(e.error||JSON.stringify(e))); }
}
async function deleteWord(term){
  if(!confirm('Delete '+decodeURIComponent(term)+'?')) return;
  const r = await fetch('/word/' + decodeURIComponent(term), { method: 'DELETE' });
  if(r.ok) loadAll(); else { const e = await r.json(); alert('Error: '+(e.error||JSON.stringify(e))); }
}
loadAll();
