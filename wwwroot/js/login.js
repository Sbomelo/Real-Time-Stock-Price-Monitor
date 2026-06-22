document.getElementById("login").addEventListener("click", async () => {
    const res = await fetch("/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: document.getElementById("u").value,
                                 password: document.getElementById("p").value })
    });

    if (!res.ok) { 
        document.getElementById("err").style.display="block"; 
        return; }

    const data = await res.json();               // { token, role, name }
    sessionStorage.setItem("jwt", data.token);   // Store the JWT
    sessionStorage.setItem("role", data.role);
    sessionStorage.setItem("name", data.name);
    
    // Route to the correct dashboard based on the server-confirmed role
    const dest = data.role === "Admin" ? "admin.html"
               : data.role === "Viewer" ? "viewer.html" : "trader.html";
    location.href = dest;
});