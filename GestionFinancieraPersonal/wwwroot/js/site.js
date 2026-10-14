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

// Pantalla "Sin conexión": se muestra al perder internet o si el servidor no responde.
(function () {
    var pantalla = document.getElementById("offlineScreen");
    var btn = document.getElementById("btnReintentar");
    if (!pantalla || !btn) return;

    function mostrar() { pantalla.classList.add("visible"); }
    function ocultar() { pantalla.classList.remove("visible"); }

    function reintentar() {
        btn.disabled = true;
        fetch("/favicon.ico", { method: "HEAD", cache: "no-store" })
            .then(function (r) { if (r.ok) ocultar(); else mostrar(); })
            .catch(mostrar)
            .then(function () { btn.disabled = false; });
    }

    window.addEventListener("offline", mostrar);
    window.addEventListener("online", reintentar);
    btn.addEventListener("click", reintentar);
    if (!navigator.onLine) mostrar();
})();