(() => {
    const passwordInput = document.getElementById("password");
    const confirmInput = document.getElementById("repassword");
    const matchPassword = document.getElementById("password-match");
    const strengthBar = document.getElementById("password-strength-bar");
    const strengthText = document.getElementById("password-strength-text");

    const rules = {
        length: { regex: /.{8,}/, element: document.getElementById("req-length") },
        upper: { regex: /[A-Z]/, element: document.getElementById("req-upper") },
        lower: { regex: /[a-z]/, element: document.getElementById("req-lower") },
        number: { regex: /[0-9]/, element: document.getElementById("req-number") },
        special: { regex: /[^A-Za-z0-9]/, element: document.getElementById("req-special") }
    };

    const strengthLevels = {
        0: { color: "bg-red-500", text: "Muito fraca" },
        1: { color: "bg-red-500", text: "Muito fraca" },
        2: { color: "bg-orange-500", text: "Fraca" },
        3: { color: "bg-yellow-500", text: "Média" },
        4: { color: "bg-blue-500", text: "Boa" },
        5: { color: "bg-green-500", text: "Forte" }
    };

    function validateRules(value) {
        Object.values(rules).forEach(rule => {
            if (rule.regex.test(value)) {
                rule.element.classList.replace("text-red-600", "text-green-600");
            } else {
                rule.element.classList.replace("text-green-600", "text-red-600");
            }
        });
    }

    function validatePasswordConfirmation() {
        if (confirmInput.value === "" || passwordInput.value === confirmInput.value) {
            matchPassword.textContent = "";
            matchPassword.classList.remove("text-red-600", "text-green-600");
        } else {
            matchPassword.textContent = "As senhas não coincidem";
            matchPassword.classList.add("text-red-600");
            matchPassword.classList.remove("text-green-600");
        }
    }

    function updatePasswordStrength(value) {
        let score = 0;
        if (rules.length.regex.test(value)) score++;
        if (rules.upper.regex.test(value)) score++;
        if (rules.lower.regex.test(value)) score++;
        if (rules.number.regex.test(value)) score++;
        if (rules.special.regex.test(value)) score++;

        const percent = (score / 5) * 100;
        strengthBar.style.width = percent + "%";

        const { color, text } = strengthLevels[score];
        strengthBar.classList.remove("bg-red-500", "bg-orange-500", "bg-yellow-500", "bg-blue-500", "bg-green-500");
        strengthBar.classList.add(color);

        strengthText.textContent = `Força: ${text}`;
    }

    passwordInput.addEventListener("input", () => {
        const value = passwordInput.value;
        validateRules(value);
        updatePasswordStrength(value);
        validatePasswordConfirmation();
    });

    confirmInput.addEventListener("input", validatePasswordConfirmation);
})();