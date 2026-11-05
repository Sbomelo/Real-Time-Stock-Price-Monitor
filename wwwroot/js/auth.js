const Auth = (() => {
    const KEYS = ["jwt", "role", "name"];
    const safe = (fn) => { try { return fn(); } catch { return null; } };

    function get(key) {
        return safe(() => sessionStorage.getItem(key)) ?? safe(() => localStorage.getItem(key));
    }

    function save({ token, role, name }, remember) {
        const target = remember ? localStorage : sessionStorage;
        safe(() => {
            target.setItem("jwt", token);
            target.setItem("role", role);
            target.setItem("name", name);
        });
        if (remember) KEYS.forEach(k => safe(() => sessionStorage.removeItem(k)));
    }

    function clear() {
        KEYS.forEach(k => {
            safe(() => sessionStorage.removeItem(k));
            safe(() => localStorage.removeItem(k));
        });
    }

    function isExpired(token) {
        try {
            const b64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
            const payload = JSON.parse(atob(b64));
            return !payload.exp || payload.exp * 1000 <= Date.now();
        } catch {
            return true;
        }
    }

    function isLoggedIn() {
        const token = get("jwt");
        return !!token && !isExpired(token);
    }

    // Exact file-name case: Linux hosting is case-sensitive.
    function homeFor(role) {
        return role === "Admin" ? "Admin.html"
            : role === "Viewer" ? "Viewer.html"
                : "Trader.html";
    }

    function logout() {
        clear();
        location.href = "/";
    }

    function requireLogin(requiredRole) {
        if (!isLoggedIn()) { logout(); return false; }
        if (requiredRole && get("role") !== requiredRole) {
            location.href = homeFor(get("role"));
            return false;
        }
        return true;
    }

    function isAuthError(err) {
        return /401|unauthori[sz]ed/i.test((err && err.message) || "");
    }

    return { get, save, clear, isLoggedIn, homeFor, logout, requireLogin, isAuthError };
})();