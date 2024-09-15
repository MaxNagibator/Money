$(document).ready(function () {
    const cookieName = $('#timeZoneCookieName').val();
    if (cookieName) {
        document.cookie = `${cookieName}=${new Date().getTimezoneOffset()}; path=/`;
    }
});