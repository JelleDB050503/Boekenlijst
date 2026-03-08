const API = "";

async function apiGet(path) {
  const res = await fetch(`${API}${path}`);
  if (!res.ok) throw new Error(`GET ${path} failed`);
  return res.json();
}

async function apiSend(path, method, body) {
  const res = await fetch(`${API}${path}`, {
    method,
    headers: { "Content-Type": "application/json" },
    body: body ? JSON.stringify(body) : undefined
  });
  if (!res.ok) throw new Error(`${method} ${path} failed`);
  return res.status === 204 ? null : res.json();
}

function fullAuthorName(book) {
  return `${book.auteurVoornaam ?? ""} ${book.auteurNaam ?? ""}`.trim();
}

function findStatus(statuses, includesText) {
  const match = statuses.find(s => (s.naam || "").toLowerCase().includes(includesText));
  return match ? match.id : null;
}

async function initHomePage() {
  const [books, statuses] = await Promise.all([apiGet("/api/Boeken"), apiGet("/api/Statuses")]);
  const readIds = statuses.filter(s => (s.naam || "").toLowerCase().includes("read") || (s.naam || "").toLowerCase().includes("gelezen")).map(s => s.id);
  const tbrIds = statuses.filter(s => (s.naam || "").toLowerCase().includes("to") || (s.naam || "").toLowerCase().includes("nog")).map(s => s.id);

  document.getElementById("stat-total").textContent = books.length;
  document.getElementById("stat-read").textContent = books.filter(b => readIds.includes(b.statusId)).length;
  document.getElementById("stat-tbr").textContent = books.filter(b => tbrIds.includes(b.statusId)).length;
}

async function initBooksPage() {
  const [books, statuses] = await Promise.all([apiGet("/api/Boeken"), apiGet("/api/Statuses")]);
  const list = document.getElementById("books-list");
  const searchInput = document.getElementById("search-input");
  const statusFilter = document.getElementById("status-filter");

  statuses.forEach(s => {
    const option = document.createElement("option");
    option.value = String(s.id);
    option.textContent = s.naam;
    statusFilter.append(option);
  });

  const render = () => {
    const query = searchInput.value.trim().toLowerCase();
    const selectedStatus = statusFilter.value;
    const filtered = books.filter(b => {
      const text = `${b.titel} ${fullAuthorName(b)}`.toLowerCase();
      const statusOk = !selectedStatus || String(b.statusId) === selectedStatus;
      return text.includes(query) && statusOk;
    });

    list.innerHTML = "";
    if (filtered.length === 0) {
      list.innerHTML = "<p class='muted'>No books found.</p>";
      return;
    }

    filtered.forEach(book => {
      const card = document.createElement("article");
      card.className = "book-card";
      card.innerHTML = `
        <p class="book-title">${book.titel}</p>
        <p class="muted">${fullAuthorName(book)}</p>
        <p class="muted">Status: ${book.statusNaam}</p>
        <p class="muted">Year: ${book.jaaruitgave ?? "-"}</p>
      `;
      list.append(card);
    });
  };

  searchInput.addEventListener("input", render);
  statusFilter.addEventListener("change", render);
  render();
}

async function initManagePage() {
  const [authors, statuses] = await Promise.all([apiGet("/api/Auteurs"), apiGet("/api/Statuses")]);
  const authorSelect = document.getElementById("author-select");
  const statusSelect = document.getElementById("status-select");
  const form = document.getElementById("add-book-form");
  const message = document.getElementById("form-message");

  authors.forEach(a => {
    const option = document.createElement("option");
    option.value = String(a.id);
    option.textContent = `${a.voornaam} ${a.naam}`.trim();
    authorSelect.append(option);
  });

  statuses.forEach(s => {
    const option = document.createElement("option");
    option.value = String(s.id);
    option.textContent = s.naam;
    statusSelect.append(option);
  });

  const tbrStatusId = findStatus(statuses, "to") ?? findStatus(statuses, "nog") ?? statuses[0]?.id;
  if (tbrStatusId) {
    statusSelect.value = String(tbrStatusId);
  }

  async function loadManageList() {
    const books = await apiGet("/api/Boeken");
    const readStatusId = findStatus(statuses, "read") ?? findStatus(statuses, "gelezen");
    const tbrStatus = findStatus(statuses, "to") ?? findStatus(statuses, "nog");
    const list = document.getElementById("manage-books-list");
    list.innerHTML = "";

    books.forEach(book => {
      const row = document.createElement("div");
      row.className = "manage-row";

      const info = document.createElement("div");
      info.innerHTML = `<strong>${book.titel}</strong><p class='muted'>${fullAuthorName(book)} - ${book.statusNaam}</p>`;

      const statusBtn = document.createElement("button");
      statusBtn.className = "btn btn-ghost";
      const moveToRead = readStatusId && book.statusId !== readStatusId;
      const targetStatus = moveToRead ? readStatusId : tbrStatus;
      statusBtn.textContent = moveToRead ? "Set to Read" : "Set to To-Be-Read";
      statusBtn.disabled = !targetStatus;
      statusBtn.addEventListener("click", async () => {
        if (!targetStatus) return;
        await apiSend(`/api/Boeken/${book.id}/status`, "PUT", { statusId: targetStatus });
        await loadManageList();
      });

      const deleteBtn = document.createElement("button");
      deleteBtn.className = "btn btn-danger";
      deleteBtn.textContent = "Delete";
      deleteBtn.addEventListener("click", async () => {
        const ok = confirm(`Delete '${book.titel}'?`);
        if (!ok) return;
        await apiSend(`/api/Boeken/${book.id}`, "DELETE");
        await loadManageList();
      });

      row.append(info, statusBtn, deleteBtn);
      list.append(row);
    });
  }

  form.addEventListener("submit", async e => {
    e.preventDefault();
    const data = new FormData(form);
    const payload = {
      titel: String(data.get("titel") || "").trim(),
      auteurId: Number(data.get("auteurId")),
      statusId: Number(data.get("statusId")),
      jaaruitgave: data.get("jaaruitgave") ? Number(data.get("jaaruitgave")) : null,
      reeks: String(data.get("reeks") || "").trim() || null,
      reeksVolgorde: data.get("reeksVolgorde") ? Number(data.get("reeksVolgorde")) : null
    };

    try {
      await apiSend("/api/Boeken", "POST", payload);
      form.reset();
      message.textContent = "Book added successfully.";
      await loadManageList();
    } catch {
      message.textContent = "Could not add book. Check required fields.";
    }
  });

  await loadManageList();
}
