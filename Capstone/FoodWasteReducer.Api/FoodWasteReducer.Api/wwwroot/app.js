const el = (id) => document.getElementById(id);
const status = el('status');
const imageInput = el('imageInput');
const preview = el('preview');
const analyzeBtn = el('analyzeBtn');
const itemsList = el('itemsList');
const servings = el('servings');
const recipeBtn = el('recipeBtn');
const result = el('result');

let lastItems = [];

imageInput.addEventListener('change', () => {
  const f = imageInput.files?.[0];
  if (!f) { preview.src = ''; return; }
  const url = URL.createObjectURL(f);
  preview.src = url;
});

analyzeBtn.addEventListener('click', async () => {
  const f = imageInput.files?.[0];
  if (!f) { alert('Choose an image first'); return; }
  analyzeBtn.disabled = true; recipeBtn.disabled = true; result.textContent=''; itemsList.textContent='';
  status.textContent = 'Analyzing image...';
  try {
    const fd = new FormData();
    fd.append('image', f);
    const r = await fetch('/api/analyze-image', { method: 'POST', body: fd });
    if (!r.ok) throw new Error('Analyze failed');
    const j = await r.json();
    lastItems = (j.items||[]).map(x => x.name);
    itemsList.innerHTML = '';
    (j.items||[]).forEach(it => {
      const li = document.createElement('li');
      li.textContent = `${it.name} (${Math.round(it.confidence*100)}%)`;
      itemsList.appendChild(li);
    });
    status.textContent = 'Items detected.';
  } catch (e) {
    console.error(e);
    status.textContent = 'Analyze failed';
  } finally {
    analyzeBtn.disabled = false; recipeBtn.disabled = false;
  }
});

recipeBtn.addEventListener('click', async () => {
  if (lastItems.length === 0) { alert('Analyze an image first'); return; }
  recipeBtn.disabled = true; status.textContent = 'Generating recipe...';
  try {
    const r = await fetch('/api/generate-recipe', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ items: lastItems, servings: Number(servings.value || 2) })
    });
    if (!r.ok) throw new Error('Recipe failed');
    const j = await r.json();
    result.innerHTML = '';
    const title = document.createElement('h3');
    title.textContent = j?.title || 'Recipe';
    const ingH = document.createElement('h4'); ingH.textContent = 'Ingredients';
    const ing = document.createElement('ul');
    (j?.ingredients||[]).forEach(x => { const li = document.createElement('li'); li.textContent = x; ing.appendChild(li); });
    const stepsH = document.createElement('h4'); stepsH.textContent = 'Steps';
    const steps = document.createElement('ol');
    (j?.steps||[]).forEach(x => { const li = document.createElement('li'); li.textContent = x; steps.appendChild(li); });
    const nutH = document.createElement('h4'); nutH.textContent = `Nutrition (per serving)`;
    const nut = j?.nutrition_per_serving;
    const nutP = document.createElement('p');
    if (nut) {
      nutP.textContent = `${Math.round(nut.calories)} kcal • Protein ${Math.round(nut.protein_g)}g • Carbs ${Math.round(nut.carbs_g)}g • Fat ${Math.round(nut.fat_g)}g`;
    }
    const serveP = document.createElement('p'); serveP.textContent = `Servings: ${j?.servings ?? servings.value}`;

    result.appendChild(title);
    result.appendChild(serveP);
    result.appendChild(ingH); result.appendChild(ing);
    result.appendChild(stepsH); result.appendChild(steps);
    result.appendChild(nutH); result.appendChild(nutP);
    status.textContent = 'Recipe ready.';
  } catch (e) {
    console.error(e);
    status.textContent = 'Recipe generation failed';
  } finally {
    recipeBtn.disabled = false;
  }
});


