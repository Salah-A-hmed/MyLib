const booksData = {
  books: [
    {
      id: 1,
      collection: "Action",
      title: "Bourne Identity (Bourne Trilogy No.1)",
      author: "Robert Ludlum",
      year: 1984,
      pages: 544,
      isbn: "978-0553593549",
      image:
        "https://m.media-amazon.com/images/I/81LRac7HYXL._SL1500_.jpg",
      description:
        "Jason Bourne has no memory of who he is, but his body holds secrets—microfilm under his skin, a face changed by surgery, and a mysterious bank account. As assassins close in, he races to uncover the truth about his identity before it’s too late.",
      rating: 4.5,
      status: "completed",
      dateAdded: "2024-01-15",
    },
    {
      id: 2,
      collection: "Action",
      title: "The Bourne Supremacy (Bourne Trilogy No.2)",
      author: "Robert Ludlum",
      year: 1986,
      pages: 597,
      isbn: "978-0553263220",
      image:
        "https://m.media-amazon.com/images/I/51V-cdR6+uL.jpg",
      description:
        "When a series of political assassinations are blamed on the legendary Jason Bourne, the U.S. government must uncover the truth behind the impersonation. Bourne is forced out of hiding into a dangerous game of deception and revenge.",
      rating: 4.3,
      status: "in-progress",
      dateAdded: "2024-01-20",
    },
    {
      id: 3,
      collection: "Action",
      title: "The Hunt for Red October",
      author: "Tom Clancy",
      year: 1984,
      pages: 387,
      isbn: "978-0425240335",
      image:
        "https://m.media-amazon.com/images/I/819VOH2Xj0L._SL1500_.jpg",
      description:
        "A Soviet submarine captain defies orders and heads for the U.S. with his nuclear submarine. The Americans and Russians race to find him in this gripping Cold War thriller that launched Tom Clancy’s career.",
      rating: 4.7,
      status: "completed",
      dateAdded: "2024-02-01",
    },
    {
      id: 4,
      collection: "Action",
      title: "Without Remorse",
      author: "Tom Clancy",
      year: 1993,
      pages: 639,
      isbn: "978-0425143322",
      image:
        "https://m.media-amazon.com/images/I/817g1XC4NML._SL1500_.jpg",
      description:
        "John Kelly becomes Mr. Clark — the CIA’s most feared operative. When personal tragedy strikes, he embarks on a mission of vengeance that blurs the line between justice and cold-blooded retribution.",
      rating: 4.6,
      status: "not-begun",
      dateAdded: "2024-02-10",
    },
    {
      id: 5,
      collection: "Action",
      title: "Day of the Jackal",
      author: "Frederick Forsyth",
      year: 1971,
      pages: 384,
      isbn: "978-0099559856",
      image:
        "https://m.media-amazon.com/images/I/71yoZWjeAsL._SL1500_.jpg",
      description:
        "An English assassin, known only as ‘The Jackal,’ is hired to kill the President of France. A tense cat-and-mouse chase unfolds in this gripping classic of espionage and suspense.",
      rating: 4.8,
      status: "completed",
      dateAdded: "2024-02-15",
    },
  ],
};

// Auto-detect current page and set active state
document.addEventListener("DOMContentLoaded", function () {
  const currentPath = window.location.pathname;
  const currentPage = currentPath.split("/").pop().replace(".html", "");

  // Remove active class from all nav links
  const navLinks = document.querySelectorAll(".nav-link[data-page]");
  navLinks.forEach((link) => link.classList.remove("active"));

  // Find and activate the current page link
  let activeLink = null;

  // Check for exact page match
  activeLink = document.querySelector(`[data-page="${currentPage}"]`);

  // Fallback mappings for different URL patterns
  if (!activeLink) {
    const pageMap = {
      index: "library",
      home: "library",
      dashboard: "library",
      books: "library",
      "add-book": "add-items",
      "add-item": "add-items",
      "new-item": "add-items",
      collection: "add-collection",
      collections: "add-collection",
      "new-collection": "add-collection",
      publishing: "publish",
      upload: "publish",
      share: "publish",
      stats: "dashboards",
      analytics: "dashboards",
      reports: "dashboards",
    };

    if (pageMap[currentPage]) {
      activeLink = document.querySelector(
        `[data-page="${pageMap[currentPage]}"]`
      );
    }
  }

  // If still no match, default to library (first item)
  if (!activeLink) {
    activeLink = document.querySelector('[data-page="library"]');
  }

  // Add active class to the determined link
  if (activeLink) {
    activeLink.classList.add("active");
  }
});

// 🔹 Helper to close all dropdowns except the one you clicked
function closeAllDropdowns(except = null) {
  document
    .querySelectorAll(
      ".select-dropdown.active, .control-dropdown.active, #userDropdown.show"
    )
    .forEach((dropdown) => {
      if (dropdown !== except) {
        dropdown.classList.remove("active", "show");
      }
    });
}

// User dropdown
const userInfo = document.getElementById("userInfo");
const userDropdown = document.getElementById("userDropdown");

if (userInfo && userDropdown) {
  userInfo.addEventListener("click", function (e) {
    e.stopPropagation();
    closeAllDropdowns(userDropdown);
    userDropdown.classList.toggle("show");
    console.log("Toggled user dropdown:", userDropdown.classList);
  });
}

// Select dropdown
const selectDropdown = document.getElementById("selectDropdown");
if (selectDropdown) {
  const selectTrigger = selectDropdown.querySelector(".select-trigger");
  const selectOptions = selectDropdown.querySelectorAll(".select-option");

  selectTrigger.addEventListener("click", function (e) {
    e.stopPropagation();
    closeAllDropdowns(selectDropdown);
    selectDropdown.classList.toggle("active");
    console.log("Toggled selectDropdown:", selectDropdown.classList);
  });

  selectOptions.forEach((option) => {
    option.addEventListener("click", function () {
      const value = this.dataset.value;
      const text = this.textContent;
      selectTrigger.querySelector("span").textContent = text;
      selectDropdown.classList.remove("active");
      console.log("Selected option:", value, text);
    });
  });
}

// View control
const viewButton = document.getElementById("viewButton");
if (viewButton) {
  const viewDropdown = viewButton.parentElement;
  const viewOptions = viewDropdown.querySelectorAll(".select-option");

  viewButton.addEventListener("click", function (e) {
    e.stopPropagation();
    closeAllDropdowns(viewDropdown);
    viewDropdown.classList.toggle("active");
    console.log("Toggled view dropdown:", viewDropdown.classList);
  });

  viewOptions.forEach((option) => {
    option.addEventListener("click", function () {
      const value = this.dataset.value;
      const text = this.textContent;
      const icon = this.dataset.icon;

      document.getElementById("viewText").textContent = text;
      viewButton.querySelector("i:first-child").className = icon;
      viewDropdown.classList.remove("active");
      console.log("View option selected:", value, text);
    });
  });
}

// Sort control
const sortButton = document.getElementById("sortButton");
if (sortButton) {
  const sortDropdown = sortButton.parentElement;
  const sortOptions = sortDropdown.querySelectorAll(".select-option");

  sortButton.addEventListener("click", function (e) {
    e.stopPropagation();
    closeAllDropdowns(sortDropdown);
    sortDropdown.classList.toggle("active");
    console.log("Toggled sort dropdown:", sortDropdown.classList);
  });

  sortOptions.forEach((option) => {
    option.addEventListener("click", function () {
      const value = this.dataset.value;
      const text = this.textContent;
      const icon = this.dataset.icon;

      document.getElementById("sortText").textContent = text;
      sortButton.querySelector("i:first-child").className = icon;
      sortDropdown.classList.remove("active");
      console.log("Sort option selected:", value, text);
    });
  });
}

// Filter sidebar functionality
const filterButton = document.getElementById("filterButton");
const filterSidebar = document.getElementById("filterSidebar");
const filterOverlay = document.getElementById("filterOverlay");
const filterClose = document.getElementById("filterClose");

filterButton.addEventListener("click", function () {
  filterSidebar.classList.add("show");
  filterOverlay.classList.add("show");
  document.body.style.overflow = "hidden";
});

function closeFilter() {
  filterSidebar.classList.remove("show");
  filterOverlay.classList.remove("show");
  document.body.style.overflow = "auto";
}

filterClose.addEventListener("click", closeFilter);
filterOverlay.addEventListener("click", closeFilter);

// // Tags toggle functionality
// const tagsToggle = document.getElementById("tagsToggle");
// tagsToggle.addEventListener("click", function () {
//   this.classList.toggle("active");
// });

// Close dropdowns when clicking outside
document.addEventListener("click", function (e) {
  closeAllDropdowns();
});

// Prevent dropdown close when clicking inside
document.querySelectorAll(".select-dropdown-content").forEach((content) => {
  content.addEventListener("click", function (e) {
    e.stopPropagation();
  });
});

// Fetch data from books_data.json file
// async function fetchBooksData() {
//   try {
//     const response = await fetch('./books_data.json');
//     if (!response.ok) {
//       throw new Error('Network response was not ok ' + response.statusText);
//     }
//     const data = await response.json();
//     console.log('Books Data:', data);
//     return data;
//   }
//   catch (error) {
//     console.error('There has been a problem with your fetch operation:', error);
//   }
// }

// Function to create book card HTML
function createBookCard(book, viewMode = "summary") {
  if (viewMode === "cover") {
    return `
      <div class="flex flex-col items-center text-center bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow duration-300 p-4">
        <img 
          src="${book.image}" 
          alt="${book.title}" 
          class="w-36 h-52 object-cover rounded shadow-md mb-3"
          onerror="this.src='https://via.placeholder.com/144x208/e5e7eb/6b7280?text=No+Image'"
        />
        <h3 class="text-sm font-semibold text-gray-900 mb-1">${book.title.slice(0, 20)}${book.title.length > 20 ? '...' : ''}</h3>
        <p class="text-xs text-gray-600">${book.author}</p>
      </div>
    `;
  }

  if (viewMode === "list") {
    return `
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow duration-300 p-4">
        <div>
          <h3 class="text-lg font-semibold text-gray-900">${book.title}</h3>
          <p class="text-sm text-gray-600 text-left">${book.author}</p>
        </div>
        <div class="text-sm text-gray-700 mt-2 sm:mt-0 text-left ">
          <span class="block"><span class="font-semibold text-black">EAN / ISBN13:</span> ${book.isbn.replace(
            /-/g,
            ""
          )}</span>
          <span class="block"><span class="font-semibold text-black">UPC / ISBN10:</span> ${book.isbn.slice(
            -10
          )}</span>
        </div>
      </div>
    `;
  }

  // Default: Summary view (your current detailed layout)
  return `
    <div class="bg-white border border-gray-200 rounded-lg shadow-sm hover:shadow-md transition-shadow duration-300 p-5 mb-8">
      <div class="flex flex-col md:flex-row gap-6">
        <!-- Book Cover -->
        <div class="flex-shrink-0">
          <img 
            src="${book.image}" 
            alt="${book.title}" 
            class="w-32 h-48 object-cover rounded shadow-md"
            onerror="this.src='https://via.placeholder.com/128x192/e5e7eb/6b7280?text=No+Image'"
          />
        </div>
        
        <!-- Book Details -->
        <div class="flex-grow">
          <div class="flex justify-between items-start">
            <div>
              <h3 class="text-xl md:text-2xl font-bold text-gray-900 mb-1">${
                book.title
              }</h3>
              <p class="text-base md:text-lg text-gray-600 text-left">${
                book.author
              }</p>
            </div>
            <button class="text-gray-400 hover:text-gray-600 transition-colors">
              <i class="fas fa-edit text-xl"></i>
            </button>
          </div>
          
          <div class="flex flex-wrap gap-3 text-sm text-gray-600 mb-3 text-left">
            <span class="font-semibold text-black">${book.year}</span>
            <span>${book.pages} pages</span>
            <span>(Bantam)</span>
          </div>

          <div class="text-sm text-gray-700 mb-3 text-left">
            <span class="font-semibold text-black">EAN / ISBN13:</span> ${book.isbn.replace(
              /-/g,
              ""
            )}
            <span class="ml-4"><span class="font-semibold text-black">UPC / ISBN10:</span> ${book.isbn.slice(
              -10
            )}</span>
          </div>

          <div class="text-sm text-gray-700 mb-3 text-left">
            <span class="font-semibold text-black">Added:</span> ${
              book.dateAdded
            }
          </div>

          <div class="text-sm text-gray-700 mb-4 text-left">
            <span class="font-semibold text-black">Copies:</span> 1
          </div>

          <div class="border-t border-gray-200 pt-4 text-left">
            <h4 class="text-base font-semibold text-gray-900 mb-2 border-b-2 border-cyan-400 inline-block pb-1">Description</h4>
            <p class="text-xs text-gray-700 leading-relaxed text-left">${
              book.description
            }</p>
          </div>
        </div>
      </div>
    </div>
  `;
}

// Adjust parent content-area if needed
function adjustContentArea() {
  const contentArea = document.querySelector(".content-area");
  if (contentArea) {
    contentArea.style.padding = "20px";
    contentArea.style.overflowY = "auto";
    console.log("Content area adjusted");
  } else {
    console.error("Content area not found");
  }
}

// --- Helper: ensure modal exists (creates it if missing) ---
function ensureBookModalExists() {
  if (document.getElementById('bookModal')) return;

  const modalHtml = `
  <div class="modal fade" id="bookModal" tabindex="-1" aria-labelledby="bookModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-fullscreen">
      <div class="modal-content bg-white text-gray-800">
        <div class="modal-header flex justify-between items-center border-b border-gray-200">
          <div class="flex items-center gap-5 px-4">
            <button id="modalEditBtn" class="text-gray-700 hover:text-blue-600 flex items-center gap-2">
              <i class="bi bi-pencil"></i> Edit
            </button>
            <button id="modalDetailsBtn" class="text-gray-700 hover:text-blue-600 flex items-center gap-2">
              <i class="bi bi-info-circle"></i> Details
            </button>
            <button id="modalDeleteBtn" class="text-red-600 hover:text-red-800 flex items-center gap-2">
              <i class="bi bi-trash"></i> Delete
            </button>
          </div>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>

        <div class="modal-body flex flex-col lg:flex-row gap-10 px-10 py-8 overflow-y-auto">
          <div class="flex-shrink-0">
            <img id="modalImage" src="" alt="" class="w-64 rounded-lg shadow-md" />
          </div>

          <div class="flex-grow">
            <h1 id="modalTitle" class="text-3xl font-bold mb-2"></h1>
            <h2 id="modalAuthor" class="text-lg text-gray-600 mb-3"></h2>

            <p class="text-sm mb-1">
              <strong>Year:</strong> <span id="modalYear"></span> &nbsp; | &nbsp;
              <strong>Pages:</strong> <span id="modalPages"></span> &nbsp; | &nbsp;
              <strong>Publisher:</strong> <span id="modalPublisher"></span>
            </p>

            <p class="text-sm mb-3">
              <strong>ISBN13:</strong> <span id="modalISBN13"></span> &nbsp; | &nbsp;
              <strong>ISBN10:</strong> <span id="modalISBN10"></span>
            </p>

            <p class="text-sm mb-5">
              <strong>Added:</strong> <span id="modalAdded"></span> &nbsp; | &nbsp;
              <strong>Copies:</strong> <span id="modalCopies"></span>
            </p>

            <div>
              <h3 class="text-xl font-semibold border-b-2 border-blue-400 inline-block mb-3">Description</h3>
              <p id="modalDescription" class="leading-relaxed text-gray-700 text-justify max-w-[750px]"></p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>`;

  document.body.insertAdjacentHTML('beforeend', modalHtml);
}

// --- Helper: populate modal fields and show it (Bootstrap) ---
function openBookModal(book, index = 0) {
  // fill modal fields (use fallbacks for older keys)
  const image = book.image || book.cover || '';
  const title = book.title || '';
  const author = book.author || '';
  const year = book.year || '';
  const pages = book.pages || '';
  const publisher = book.publisher || book.collection || '';
  const isbnRaw = book.isbn || book.isbn13 || '';
  const isbn13 = (isbnRaw && isbnRaw.replace) ? isbnRaw.replace(/-/g, '') : (book.isbn13 || '');
  const isbn10 = book.isbn10 || (typeof isbnRaw === 'string' ? isbnRaw.slice(-10) : '');
  const added = book.dateAdded || book.added || '';
  const copies = book.copies ?? 1;
  const description = book.description || '';

  const setText = (id, txt) => {
    const el = document.getElementById(id);
    if (el) el.textContent = txt ?? '';
  };

  const setAttr = (id, attr, val) => {
    const el = document.getElementById(id);
    if (el) el.setAttribute(attr, val ?? '');
  };

  setAttr('modalImage', 'src', image);
  setAttr('modalImage', 'alt', title);
  setText('modalTitle', title);
  setText('modalAuthor', author);
  setText('modalYear', year);
  setText('modalPages', pages);
  setText('modalPublisher', publisher);
  setText('modalISBN13', isbn13);
  setText('modalISBN10', isbn10);
  setText('modalAdded', added);
  setText('modalCopies', copies);
  setText('modalDescription', description);

  // Add data-index to action buttons so you can know which book they belong to
  const btns = ['modalEditBtn', 'modalDetailsBtn', 'modalDeleteBtn'];
  btns.forEach(id => {
    const b = document.getElementById(id);
    if (b) b.dataset.index = index;
  });

  // Show the modal using Bootstrap JS API
  const modalEl = document.getElementById('bookModal');
  if (modalEl && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
    const bsModal = new bootstrap.Modal(modalEl);
    bsModal.show();
  } else {
    // fallback to toggling if bootstrap not available globally
    const evt = new Event('show.bs.modal');
    modalEl && modalEl.dispatchEvent(evt);
  }
}

// --- Replaced initializeBooks (drop this into your JS file) ---
function initializeBooks() {
  let currentViewMode = "summary";
  const bookContainer = document.querySelector(".book");

  if (!bookContainer) {
    console.error("Book container not found!");
    return;
  }

  // ensure modal exists
  ensureBookModalExists();

  // Adjust container styles
  bookContainer.style.width = "100%";
  bookContainer.style.maxWidth = "1400px";
  bookContainer.style.margin = "0 auto";
  bookContainer.style.padding = "20px";

  // render function (wrap each card with data-index)
  function renderBooks() {
    if (!booksData || !booksData.books || booksData.books.length === 0) {
      bookContainer.innerHTML =
        '<div style="padding: 20px; text-align: center; color: #666;">No books available</div>';
      return;
    }

    // Build cards and wrap with a .book-item wrapper carrying data-index
    const booksHTML = booksData.books
      .map((book, i) => `<div class="book-item" data-index="${i}">${createBookCard(book, currentViewMode)}</div>`)
      .join("");
    bookContainer.innerHTML = booksHTML;

    // When in 'cover' view make parent a grid
    if (currentViewMode === "cover") {
      bookContainer.classList.add("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");
    } else {
      bookContainer.classList.remove("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");
    }

    // Attach click handlers: open modal when clicking the image or title
    // We query each .book-item and delegate inside it.
    document.querySelectorAll('.book-item').forEach(item => {
      const idx = item.dataset.index;
      const book = booksData.books[idx];

      // clickable image inside the card (any <img>)
      const img = item.querySelector('img');
      if (img) {
        img.style.cursor = 'pointer';
        img.addEventListener('click', (e) => {
          e.stopPropagation();
          openBookModal(book, idx);
        });
      }

      // clickable title: try h3 or h2 inside card
      const titleEl = item.querySelector('h3, h2');
      if (titleEl) {
        titleEl.style.cursor = 'pointer';
        titleEl.addEventListener('click', (e) => {
          e.stopPropagation();
          openBookModal(book, idx);
        });
      }

      // If you want the whole card to open modal (uncomment):
      // item.style.cursor = 'pointer';
      // item.addEventListener('click', () => openBookModal(book, idx));
    });

    console.log(`Rendered ${booksData.books.length} books in ${currentViewMode} view`);
  }

  // initial render
  renderBooks();

  // hook dropdown options to change view mode
  document.querySelectorAll(".select-option").forEach((option) => {
    option.addEventListener("click", () => {
      currentViewMode = option.dataset.value; // "cover", "list", or "summary"
      renderBooks();
    });
  });

  // Hook modal action buttons (Edit/Details/Delete) to user-provided handlers if desired
  document.addEventListener('click', (e) => {
    const target = e.target.closest('#modalEditBtn, #modalDetailsBtn, #modalDeleteBtn');
    if (!target) return;
    const idx = target.dataset.index;
    const book = booksData.books[idx];

    if (target.id === 'modalEditBtn') {
      console.log('Edit clicked for index', idx, book);
      // call your edit function or redirect: e.g. window.location = `/edit.html?id=${book.id}`
    } else if (target.id === 'modalDetailsBtn') {
      console.log('Details clicked for index', idx, book);
      // open details route or show extra info
    } else if (target.id === 'modalDeleteBtn') {
      console.log('Delete clicked for index', idx, book);
      // confirm then delete
      openDeleteModal(book, () => {
    booksData.books.splice(idx, 1);
    renderBooks();
    const modalEl = document.getElementById('bookModal');
    if (modalEl && window.bootstrap && typeof bootstrap.Modal === 'function') {
      bootstrap.Modal.getInstance(modalEl)?.hide();
    }
  });
    }
  });
}

// --- DELETE CONFIRMATION MODAL (Bootstrap + Tailwind styling) ---
function ensureDeleteModalExists() {
  if (document.getElementById("deleteConfirmModal")) return;

  const modalHtml = `
  <div class="modal fade" id="deleteConfirmModal" tabindex="-1" aria-labelledby="deleteConfirmLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content rounded-2xl shadow-lg">
        <div class="modal-body text-center p-6">
          <h5 class="text-lg font-semibold text-gray-900 mb-4">Delete this item?</h5>
          <div class="flex justify-center gap-3">
            <button id="confirmDeleteBtn" class="btn btn-danger px-4 py-2 text-white rounded-lg">Delete</button>
            <button id="cancelDeleteBtn" class="btn btn-light px-4 py-2 border rounded-lg">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  </div>`;
  document.body.insertAdjacentHTML("beforeend", modalHtml);
}

function openDeleteModal(book, onConfirm) {
  ensureDeleteModalExists();

  const modalEl = document.getElementById("deleteConfirmModal");
  const confirmBtn = modalEl.querySelector("#confirmDeleteBtn");
  const cancelBtn = modalEl.querySelector("#cancelDeleteBtn");

  const bsModal = new bootstrap.Modal(modalEl);
  bsModal.show();

  // clear previous listeners to avoid stacking
  confirmBtn.replaceWith(confirmBtn.cloneNode(true));
  cancelBtn.replaceWith(cancelBtn.cloneNode(true));

  const newConfirmBtn = modalEl.querySelector("#confirmDeleteBtn");
  const newCancelBtn = modalEl.querySelector("#cancelDeleteBtn");

  newConfirmBtn.addEventListener("click", () => {
    bsModal.hide();
    if (typeof onConfirm === "function") onConfirm(book);
  });

  newCancelBtn.addEventListener("click", () => bsModal.hide());
}


// Wait for DOM to be ready
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM loaded, initializing...");
    adjustContentArea();
    initializeBooks();
  });
} else {
  console.log("DOM already loaded, initializing...");
  adjustContentArea();
  initializeBooks();
}
