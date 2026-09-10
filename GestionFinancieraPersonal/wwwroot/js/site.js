// Toggle del menú lateral en vista móvil (compartido por usuario y admin).
document.addEventListener("DOMContentLoaded", function () {
    var btnAbrir = document.getElementById("btnMenu");
    var btnCerrar = document.getElementById("btnCerrarMenu");
    var sidebar = document.getElementById("sidebarMenu");
    var overlay = document.getElementById("menuOverlay");

    function abrirMenu() {
        if (sidebar) sidebar.classList.add("open");
        if (overlay) overlay.classList.add("visible");
    }
    function cerrarMenu() {
        if (sidebar) sidebar.classList.remove("open");
        if (overlay) overlay.classList.remove("visible");
    }

    if (btnAbrir) btnAbrir.addEventListener("click", abrirMenu);
    if (btnCerrar) btnCerrar.addEventListener("click", cerrarMenu);
    if (overlay) overlay.addEventListener("click", cerrarMenu);
});
