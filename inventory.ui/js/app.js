const apiBase = 'http://localhost:5000/api';
let token = null;

function el(id){return document.getElementById(id)}

async function login(e){
  e && e.preventDefault();
  const username = el('username').value.trim();
  const password = el('password').value.trim();
  if(!username||!password){alert('Enter username and password');return}
  try{
    const res = await fetch(`${apiBase}/Auth/login`,{
      method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({username, password})
    });
    if(!res.ok){const txt=await res.text(); throw new Error(txt||res.status)}
    const data = await res.json();
    token = data.token;
    localStorage.setItem('jwt', token);
    el('loginSection').classList.add('hidden');
    el('appSection').classList.remove('hidden');
    // show user and logout
    const userEl = el('userDisplay');
    if(userEl) userEl.classList.remove('hidden');
    const logoutBtn = el('btnLogout');
    if(logoutBtn) logoutBtn.classList.remove('hidden');
    if(userEl) userEl.textContent = data.username || data.email || '';
    await loadStock();
  }catch(err){console.error(err);alert('Login failed: '+err.message)}
}

async function loadStock(){
  try{
    const res = await fetch(`${apiBase}/Stock`,{headers:{'Authorization':'Bearer '+token}});
    if(!res.ok){if(res.status===401){alert('Unauthorized - please login again'); logout();} else throw new Error('Failed to load stock')}
    const items = await res.json();
    renderStock(items);
  }catch(err){console.error(err);alert('Could not load stock: '+err.message)}
}

function renderStock(items){
  // find tbody in the stock table
  const tbody = document.querySelector('#stockTable tbody');
  if(!tbody) return;
  tbody.innerHTML = '';
  items.forEach(it=>{
    const tr = document.createElement('tr');
    tr.innerHTML = `<td>${escapeHtml(it.sku||'')}</td><td>${escapeHtml(it.name||'')}</td><td>${escapeHtml(it.category||'')}</td><td>${(it.quantity||0).toFixed(0)}</td><td>${(it.sellingPrice||0).toFixed(2)}</td>`;
    tbody.appendChild(tr);
  });
}

function escapeHtml(s){
  if(!s) return '';
  return String(s).replace(/[&<>\"'/]/g, c=>({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":"&#39;","/":"&#47;"
  }[c]));
}

function logout(){
  token=null;localStorage.removeItem('jwt');
  el('appSection').classList.add('hidden');
  el('loginSection').classList.remove('hidden');
  const userEl = el('userDisplay'); if(userEl){userEl.textContent='';userEl.classList.add('hidden');}
  const logoutBtn = el('btnLogout'); if(logoutBtn) logoutBtn.classList.add('hidden');
}

function init(){
  document.querySelector('#loginForm').addEventListener('submit', login);
  const btnLogout = document.querySelector('#btnLogout');
  if(btnLogout) btnLogout.addEventListener('click', logout);
  const btnRefresh = document.querySelector('#btnRefresh');
  if(btnRefresh) btnRefresh.addEventListener('click', loadStock);
  const btnNew = document.querySelector('#btnNew');
  if(btnNew){
    btnNew.addEventListener('click', ()=>{
      const modal = el('newItemModal'); if(modal) modal.classList.remove('hidden');
    });
  }
  const btnCancel = document.querySelector('#btnCancel');
  if(btnCancel) btnCancel.addEventListener('click', ()=>{const modal=el('newItemModal'); if(modal) modal.classList.add('hidden');});

  const newItemForm = document.querySelector('#newItemForm');
  if(newItemForm){
    newItemForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      const payload = {
        sku: el('ni_sku').value.trim(),
        name: el('ni_name').value.trim(),
        category: el('ni_cat').value.trim(),
        quantity: Number(el('ni_qty').value) || 0,
        sellingPrice: Number(el('ni_price').value) || 0,
        wholesalePrice: Number(el('ni_wprice').value) || 0
      };
      try{
        const res = await fetch(`${apiBase}/Stock`,{method:'POST',headers:{'Content-Type':'application/json','Authorization':'Bearer '+token},body:JSON.stringify(payload)});
        if(!res.ok){const txt=await res.text();throw new Error(txt||res.status)}
        const created = await res.json();
        const modal = el('newItemModal'); if(modal) modal.classList.add('hidden');
        await loadStock();
      }catch(err){console.error(err);alert('Create failed: '+err.message)}
    });
  }

  const saved = localStorage.getItem('jwt');
  if(saved){token=saved;el('loginSection').classList.add('hidden');el('appSection').classList.remove('hidden');
    const userEl = el('userDisplay'); if(userEl){userEl.classList.remove('hidden'); userEl.textContent='me';}
    const logoutBtnEl = el('btnLogout'); if(logoutBtnEl) logoutBtnEl.classList.remove('hidden');
    loadStock();
  }
}

window.addEventListener('DOMContentLoaded', init);
