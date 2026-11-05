if (Auth.isLoggedIn()) {
    location.href = Auth.homeFor(Auth.get("role"));
}

document.getElementById("login").addEventListener("click", async () => {
    const res = await fetch("/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            username: document.getElementById("u").value,
            password: document.getElementById("p").value
        })
    });

    if (!res.ok) {
        document.getElementById("err").style.display = "block";
        return;
    }

    const data = await res.json();
    Auth.save(data, document.getElementById("remember").checked);
    location.href = Auth.homeFor(data.role);
});