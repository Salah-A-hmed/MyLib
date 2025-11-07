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

document
  .getElementById("collectionForm")
  .addEventListener("submit", function (e) {
    e.preventDefault();
    let valid = true;

    // Clear old errors
    document
      .querySelectorAll(".error")
      .forEach((el) => el.classList.remove("error"));
    document
      .querySelectorAll(".error-message")
      .forEach((el) => (el.textContent = ""));

    // Validate Collection
    const collection = document.getElementById("collection");
    if (!collection.value) {
      collection.classList.add("error");
      collection.nextElementSibling.textContent = "Please select a collection.";
      valid = false;
    }

    // Validate Item Type (radio)
    const radios = document.querySelectorAll("input[name='itemType']");
    if (![...radios].some((r) => r.checked)) {
      document.querySelector("#itemTypeGroup + .error-message").textContent =
        "Please select an item type.";
      valid = false;
    }

    // Validate Language
    const language = document.getElementById("language");
    if (!language.value) {
      language.classList.add("error");
      language.nextElementSibling.textContent = "Please select a language.";
      valid = false;
    }
  });
