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

  const gastoForm = document.querySelector("[data-gasto-form]");
  const allowedGastoExtensions = ["pdf", "jpg", "jpeg", "png"];

  window.mostrarArchivoSeleccionado = (input) => {
    const file = input.files?.[0];
    const preview = document.querySelector("[data-file-preview]");

    setFieldError(input, "");

    if (!preview) {
      return;
    }

    if (!file) {
      preview.classList.remove("show");
      preview.innerHTML = "";
      return;
    }

    const size = file.size ? `${(file.size / 1024).toFixed(1)} KB` : "Tamaño no disponible";

    preview.innerHTML = `
      <div>
        <strong>${file.name}</strong>
        <span>${size}</span>
      </div>
      <button class="btn btn-outline-danger btn-sm" type="button" onclick="quitarArchivoSeleccionado()">Quitar archivo</button>
    `;
    preview.classList.add("show");
  };

  window.quitarArchivoSeleccionado = () => {
    const input = document.querySelector("[data-gasto-file]");
    const preview = document.querySelector("[data-file-preview]");

    if (input) {
      input.value = "";
      setFieldError(input, "");
    }

    if (preview) {
      preview.classList.remove("show");
      preview.innerHTML = "";
    }
  };

  window.validarFormularioGasto = () => {
    if (!gastoForm) {
      return true;
    }

    let isValid = true;
    const summary = gastoForm.querySelector("[data-gasto-summary]");

    gastoForm.querySelectorAll("[data-gasto-required]").forEach((field) => {
      const value = field.value.trim();

      if (!value) {
        setFieldError(field, "Campo requerido.");
        isValid = false;
      } else {
        setFieldError(field, "");
      }
    });

    const monto = gastoForm.querySelector("[data-gasto-monto]");
    const montoValue = Number(monto?.value ?? 0);

    if (monto && (!monto.value || montoValue <= 0)) {
      setFieldError(monto, "El monto debe ser mayor a 0.");
      isValid = false;
    }

    const fileInput = gastoForm.querySelector("[data-gasto-file]");
    const file = fileInput?.files?.[0];

    if (fileInput && file) {
      const extension = file.name.split(".").pop()?.toLowerCase() ?? "";

      if (!allowedGastoExtensions.includes(extension)) {
        setFieldError(fileInput, "Formato permitido: pdf, jpg, jpeg o png.");
        isValid = false;
      }
    }

    if (summary) {
      summary.textContent = isValid
        ? "Gasto mock validado correctamente. No se guardaron datos."
        : "Revisa los campos marcados antes de continuar.";
      summary.classList.toggle("show", true);
      summary.classList.toggle("success", isValid);
    }

    return isValid;
  };

  if (gastoForm) {
    gastoForm.addEventListener("submit", (event) => {
      event.preventDefault();
      window.validarFormularioGasto();
    });
  }

  const pagoForm = document.querySelector("[data-pago-form]");
  const allowedPagoExtensions = ["pdf", "jpg", "jpeg", "png"];

  window.mostrarComprobanteSeleccionado = (input) => {
    const file = input.files?.[0];
    const preview = document.querySelector("[data-payment-file-preview]");

    setFieldError(input, "");

    if (!preview) {
      return;
    }

    if (!file) {
      preview.classList.remove("show");
      preview.innerHTML = "";
      return;
    }

    const size = file.size ? `${(file.size / 1024).toFixed(1)} KB` : "Tamaño no disponible";

    preview.innerHTML = `
      <div>
        <strong>${file.name}</strong>
        <span>${size}</span>
      </div>
      <button class="btn btn-outline-danger btn-sm" type="button" onclick="quitarComprobanteSeleccionado()">Quitar archivo</button>
    `;
    preview.classList.add("show");
  };

  window.quitarComprobanteSeleccionado = () => {
    const input = document.querySelector("[data-pago-file]");
    const preview = document.querySelector("[data-payment-file-preview]");

    if (input) {
      input.value = "";
      setFieldError(input, "");
    }

    if (preview) {
      preview.classList.remove("show");
      preview.innerHTML = "";
    }
  };

  window.validarFormularioPago = () => {
    if (!pagoForm) {
      return true;
    }

    let isValid = true;
    const summary = pagoForm.querySelector("[data-pago-summary]");

    pagoForm.querySelectorAll("[data-pago-required]").forEach((field) => {
      const value = field.value.trim();

      if (!value) {
        setFieldError(field, "Campo requerido.");
        isValid = false;
      } else {
        setFieldError(field, "");
      }
    });

    const monto = pagoForm.querySelector("[data-pago-monto]");
    const montoValue = Number(monto?.value ?? 0);

    if (monto && (!monto.value || montoValue <= 0)) {
      setFieldError(monto, "El monto debe ser mayor a 0.");
      isValid = false;
    } else if (monto) {
      setFieldError(monto, "");
    }

    const fileInput = pagoForm.querySelector("[data-pago-file]");
    const file = fileInput?.files?.[0];

    if (fileInput && !file) {
      setFieldError(fileInput, "El comprobante es obligatorio.");
      isValid = false;
    }

    if (fileInput && file) {
      const extension = file.name.split(".").pop()?.toLowerCase() ?? "";

      if (!allowedPagoExtensions.includes(extension)) {
        setFieldError(fileInput, "Formato permitido: pdf, jpg, jpeg o png.");
        isValid = false;
      }
    }

    if (summary) {
      summary.textContent = isValid
        ? "Pago mock validado correctamente. No se enviaron datos."
        : "Revisa los campos marcados antes de continuar.";
      summary.classList.toggle("show", true);
      summary.classList.toggle("success", isValid);
    }

    return isValid;
  };

  if (pagoForm) {
    pagoForm.addEventListener("submit", (event) => {
      event.preventDefault();
      window.validarFormularioPago();
    });
  }

  const expensasForm = document.querySelector("[data-expensas-form]");
  const expensasMock = {
    "1": { total: "$1.245.000", uf: "15", montoUf: "$83.000", gastos: "6" },
    "2": { total: "$1.680.000", uf: "24", montoUf: "$70.000", gastos: "8" },
    "3": { total: "$2.420.000", uf: "32", montoUf: "$75.625", gastos: "9" }
  };

  window.actualizarResumenExpensasMock = () => {
    if (!expensasForm) {
      return;
    }

    const consorcio = expensasForm.querySelector("[name='ConsorcioId']")?.value ?? "1";
    const data = expensasMock[consorcio] ?? expensasMock["1"];

    const total = document.querySelector("[data-expensas-total]");
    const uf = document.querySelector("[data-expensas-uf]");
    const montoUf = document.querySelector("[data-expensas-monto-uf]");
    const gastos = document.querySelector("[data-expensas-gastos]");

    if (total) total.textContent = data.total;
    if (uf) uf.textContent = data.uf;
    if (montoUf) montoUf.textContent = data.montoUf;
    if (gastos) gastos.textContent = data.gastos;
  };

  window.validarGeneracionExpensas = (previewOnly = false) => {
    if (!expensasForm) {
      return true;
    }

    let isValid = true;
    const summary = expensasForm.querySelector("[data-expensas-summary]");

    expensasForm.querySelectorAll("[data-expensas-required]").forEach((field) => {
      const value = field.value.trim();

      if (!value) {
        setFieldError(field, "Campo requerido.");
        isValid = false;
      } else {
        setFieldError(field, "");
      }
    });

    const fechaEmision = expensasForm.querySelector("[name='FechaEmision']");
    const fechaVencimiento = expensasForm.querySelector("[name='FechaVencimiento']");

    if (fechaEmision?.value && fechaVencimiento?.value) {
      const emision = new Date(`${fechaEmision.value}T00:00:00`);
      const vencimiento = new Date(`${fechaVencimiento.value}T00:00:00`);

      if (vencimiento <= emision) {
        setFieldError(fechaVencimiento, "El vencimiento debe ser posterior a la emision.");
        isValid = false;
      }
    }

    if (summary) {
      summary.textContent = isValid
        ? (previewOnly
          ? "Vista previa mock validada. No se generaron expensas."
          : "Generacion mock validada. No se guardaron datos.")
        : "Revisa los campos marcados antes de continuar.";
      summary.classList.toggle("show", true);
      summary.classList.toggle("success", isValid);
    }

    return isValid;
  };

  if (expensasForm) {
    expensasForm.addEventListener("submit", (event) => {
      event.preventDefault();
      window.validarGeneracionExpensas(false);
    });
  }

  const comunicadoForm = document.querySelector("[data-comunicado-form]");
  const allowedComunicadoExtensions = ["pdf", "jpg", "jpeg", "png"];

  window.mostrarAdjuntoComunicadoSeleccionado = (input) => {
    const file = input.files?.[0];
    const preview = document.querySelector("[data-comunicado-file-preview]");

    setFieldError(input, "");

    if (!preview) {
      return;
    }

    if (!file) {
      preview.classList.remove("show");
      preview.innerHTML = "";
      return;
    }

    const size = file.size ? `${(file.size / 1024).toFixed(1)} KB` : "Tamaño no disponible";

    preview.innerHTML = `
      <div>
        <strong>${file.name}</strong>
        <span>${size}</span>
      </div>
      <button class="btn btn-outline-danger btn-sm" type="button" onclick="quitarAdjuntoComunicadoSeleccionado()">Quitar archivo</button>
    `;
    preview.classList.add("show");
  };

  window.quitarAdjuntoComunicadoSeleccionado = () => {
    const input = document.querySelector("[data-comunicado-file]");
    const preview = document.querySelector("[data-comunicado-file-preview]");

    if (input) {
      input.value = "";
      setFieldError(input, "");
    }

    if (preview) {
      preview.classList.remove("show");
      preview.innerHTML = "";
    }
  };

  window.validarFormularioComunicado = (draftOnly = false) => {
    if (!comunicadoForm) {
      return true;
    }

    let isValid = true;
    const summary = comunicadoForm.querySelector("[data-comunicado-summary]");

    comunicadoForm.querySelectorAll("[data-comunicado-required]").forEach((field) => {
      const value = field.value.trim();

      if (!value) {
        setFieldError(field, "Campo requerido.");
        isValid = false;
      } else {
        setFieldError(field, "");
      }
    });

    const fileInput = comunicadoForm.querySelector("[data-comunicado-file]");
    const file = fileInput?.files?.[0];

    if (fileInput && file) {
      const extension = file.name.split(".").pop()?.toLowerCase() ?? "";

      if (!allowedComunicadoExtensions.includes(extension)) {
        setFieldError(fileInput, "Formato permitido: pdf, jpg, jpeg o png.");
        isValid = false;
      }
    }

    if (summary) {
      summary.textContent = isValid
        ? (draftOnly
          ? "Borrador mock validado. No se guardaron datos."
          : "Comunicado mock validado. No se enviaron datos.")
        : "Revisa los campos marcados antes de continuar.";
      summary.classList.toggle("show", true);
      summary.classList.toggle("success", isValid);
    }

    return isValid;
  };

  if (comunicadoForm) {
    comunicadoForm.addEventListener("submit", (event) => {
      event.preventDefault();
      window.validarFormularioComunicado(false);
    });
  }
})();
