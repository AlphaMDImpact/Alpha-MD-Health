
window.localDate = () => {
    var ldCurrentDate = new Date();
    return ldCurrentDate.getFullYear() +
        "-" + String(ldCurrentDate.getMonth() + 1).padStart(2, '0') +
        "-" + String(ldCurrentDate.getDate()).padStart(2, '0') +
        "T" +
        String(ldCurrentDate.getHours()).padStart(2, '0') +
        ":" + String(ldCurrentDate.getMinutes()).padStart(2, '0') +
        ":" + String(ldCurrentDate.getSeconds()).padStart(2, '0');
}; window.utcDate = () => {
    var ldCurrentDate = new Date();
    return ldCurrentDate.getUTCFullYear() +
        "-" + String(ldCurrentDate.getUTCMonth() + 1).padStart(2, '0') +
        "-" + String(ldCurrentDate.getUTCDate()).padStart(2, '0') +
        "T" +
        String(ldCurrentDate.getUTCHours()).padStart(2, '0') +
        ":" + String(ldCurrentDate.getUTCMinutes()).padStart(2, '0') +
        ":" + String(ldCurrentDate.getUTCSeconds()).padStart(2, '0');
};
window.timeZoneOffset = () => {
    return new Date().getTimezoneOffset() / 60;
};