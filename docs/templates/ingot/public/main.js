function slugifyTag(tag) {
  return tag.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
}

function collectTags(items, tagByFile) {
  for (const item of items ?? []) {
    if (item.href && item.tag) {
      const file = item.href.split('/').pop().split('#')[0];
      tagByFile.set(file, item.tag);
    }
    collectTags(item.items, tagByFile);
  }
}

function injectTocTags(tagByFile) {
  if (tagByFile.size === 0) return;

  for (const root of document.querySelectorAll('nav#toc, .sidetoc')) {
    for (const link of root.querySelectorAll('a[href]')) {
      const href = link.getAttribute('href');
      if (!href || href === '#') continue;

      const file = href.split('/').pop().split('#')[0];
      const tag = tagByFile.get(file);
      if (!tag) continue;

      let badge = link.querySelector('.ingot-toc-tag');
      if (badge?.textContent === tag && badge.classList.contains(`ingot-toc-tag--${slugifyTag(tag)}`)) {
        continue;
      }

      if (!badge) {
        badge = document.createElement('span');
        link.appendChild(badge);
      }

      badge.className = `ingot-toc-tag ingot-toc-tag--${slugifyTag(tag)}`;
      badge.textContent = tag;
    }
  }
}

async function loadTagMap() {
  const tocRel = document.querySelector('meta[name="docfx:tocrel"]')?.content;
  if (!tocRel) return new Map();

  try {
    const tocJsonUrl = new URL(tocRel.replace(/\.html$/i, '.json'), window.location.href);
    const { items } = await fetch(tocJsonUrl).then((response) => response.json());
    const tagByFile = new Map();
    collectTags(items, tagByFile);
    return tagByFile;
  } catch {
    return new Map();
  }
}

async function setupTocTags() {
  const tagByFile = await loadTagMap();
  if (tagByFile.size === 0) return;

  const apply = () => injectTocTags(tagByFile);

  apply();
  for (const delay of [100, 300, 800]) {
    setTimeout(apply, delay);
  }

  const toc = document.getElementById('toc');
  if (!toc) return;

  let scheduled = false;
  new MutationObserver(() => {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
      scheduled = false;
      apply();
    });
  }).observe(toc, { childList: true, subtree: true });
}

export default {
  defaultTheme: 'auto',
  iconLinks: [
    {
      icon: 'github',
      href: 'https://github.com/pyroboots/ingot',
      title: 'GitHub'
    }
  ],
  start: setupTocTags,
};