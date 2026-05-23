(() => {
  const body = document.body;
  const toggle = document.querySelector("[data-sidebar-toggle]");
  const overlay = document.querySelector("[data-sidebar-overlay]");

  const closeSidebar = () => body.classList.remove("sidebar-open");

  toggle?.addEventListener("click", () => {
    body.classList.toggle("sidebar-open");
  });

  overlay?.addEventListener("click", closeSidebar);

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeSidebar();
    }
  });
})();
