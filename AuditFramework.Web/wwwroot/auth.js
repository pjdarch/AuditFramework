window.auditAuth = {
  get() {
    const raw = window.sessionStorage.getItem("auditframework.auth");
    return raw ? JSON.parse(raw) : null;
  },
  set(session) {
    window.sessionStorage.setItem("auditframework.auth", JSON.stringify(session));
  },
  clear() {
    window.sessionStorage.removeItem("auditframework.auth");
  }
};
