window.quillEditors = window.quillEditors || {};

window.quillCreate = (id, initialHtml) => {
  const el = document.getElementById(id);
  if (!el) return;

  // avoid double init
  if (window.quillEditors[id]) return;

  const q = new Quill(el, { theme: "snow" });
  q.root.innerHTML = initialHtml || "";
  window.quillEditors[id] = q;
};

window.quillGetHtml = (id) => {
  const q = window.quillEditors[id];
  return q ? q.root.innerHTML : "";
};

window.quillSetHtml = (id, html) => {
  const q = window.quillEditors[id];
  if (q) q.root.innerHTML = html || "";
};