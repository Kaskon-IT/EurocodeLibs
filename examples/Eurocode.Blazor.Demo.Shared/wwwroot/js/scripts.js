// Accordion
window.toggleAccordion = function (accordionId) {
    var acc = document.getElementById(accordionId);
    acc.classList.toggle("active");
    var panel = acc.nextElementSibling;
    if (panel.style.display === "block") {
        panel.style.display = "none";
        
    } else {
        panel.style.display = "block";
    }
};

