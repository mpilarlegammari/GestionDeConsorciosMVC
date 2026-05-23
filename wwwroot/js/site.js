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

  const isValidEmail = (value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
  const loginForm = document.querySelector("[data-login-form]");

  if (loginForm) {
    const fields = {
      email: loginForm.querySelector("[name='email']"),
      password: loginForm.querySelector("[name='password']"),
      role: loginForm.querySelector("[name='role']")
    };

    const setLoginError = (name, message) => {
      const field = fields[name];
      const error = loginForm.querySelector(`[data-error-for='${name}']`);

      field?.classList.toggle("is-invalid", Boolean(message));

      if (error) {
        error.textContent = message;
      }
    };

    loginForm.addEventListener("submit", (event) => {
      event.preventDefault();

      const email = fields.email?.value.trim() ?? "";
      const password = fields.password?.value.trim() ?? "";
      const role = fields.role?.value ?? "";
      let isValid = true;

      setLoginError("email", "");
      setLoginError("password", "");
      setLoginError("role", "");

      if (!email) {
        setLoginError("email", "El email es obligatorio.");
        isValid = false;
      } else if (!isValidEmail(email)) {
        setLoginError("email", "Ingresa un email valido.");
        isValid = false;
      }

      if (!password) {
        setLoginError("password", "La contrasena es obligatoria.");
        isValid = false;
      }

      if (!role) {
        setLoginError("role", "Selecciona un rol.");
        isValid = false;
      }

      if (!isValid) {
        return;
      }

      window.location.href = role === "Propietario"
        ? "/Home/PropietarioDashboard"
        : "/Home/AdminDashboard";
    });
  }

  const consorcioForm = document.querySelector("[data-consorcio-form]");

  const setFieldError = (field, message) => {
    const container = field.closest("td, div");
    const error = container?.querySelector(`[data-error-for='${field.name}']`);

    field.classList.toggle("is-invalid", Boolean(message));

    if (error) {
      error.textContent = message;
    }
  };

  window.validarUFDuplicadas = () => {
    const rows = document.querySelectorAll("[data-uf-row]");
    const seen = new Set();
    let hasDuplicates = false;

    rows.forEach((row) => {
      const input = row.querySelector("[data-uf-numero]");
      const value = input?.value.trim().toLowerCase() ?? "";

      if (!input || !value) {
        return;
      }

      if (seen.has(value)) {
        setFieldError(input, "UF duplicada.");
        hasDuplicates = true;
      } else {
        seen.add(value);
        setFieldError(input, "");
      }
    });

    return !hasDuplicates;
  };

  window.agregarUF = () => {
    const body = document.querySelector("[data-uf-body]");
    const firstRow = body?.querySelector("[data-uf-row]");

    if (!body || !firstRow) {
      return;
    }

    const clone = firstRow.cloneNode(true);

    clone.querySelectorAll("input").forEach((input) => {
      input.value = "";
      input.classList.remove("is-invalid");
    });

    clone.querySelectorAll(".field-validation").forEach((error) => {
      error.textContent = "";
    });

    body.appendChild(clone);
  };

  window.eliminarUF = (button) => {
    const rows = document.querySelectorAll("[data-uf-row]");
    const row = button.closest("[data-uf-row]");

    if (rows.length <= 1 || !row) {
      return;
    }

    row.remove();
    window.validarUFDuplicadas();
  };

  if (consorcioForm) {
    consorcioForm.addEventListener("submit", (event) => {
      event.preventDefault();

      const summary = consorcioForm.querySelector("[data-uf-summary]");
      let isValid = true;

      consorcioForm.querySelectorAll("[data-required]").forEach((field) => {
        const value = field.value.trim();

        if (!value) {
          setFieldError(field, "Campo requerido.");
          isValid = false;
        } else {
          setFieldError(field, "");
        }
      });

      consorcioForm.querySelectorAll("[data-uf-email]").forEach((field) => {
        const value = field.value.trim();

        if (value && !isValidEmail(value)) {
          setFieldError(field, "Email invalido.");
          isValid = false;
        }
      });

      if (!window.validarUFDuplicadas()) {
        isValid = false;
      }

      if (summary) {
        summary.textContent = isValid
          ? "Formulario mock validado correctamente. No se guardaron datos."
          : "Revisa los campos marcados antes de continuar.";
        summary.classList.toggle("show", true);
        summary.classList.toggle("success", isValid);
      }
    });
  }
})();
