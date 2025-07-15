/*------------------------------------------------------------------
* Bootstrap Simple Admin Template
* Version: 3.0
* Author: Alexis Luna
* Website: https://github.com/alexis-luna/bootstrap-simple-admin-template
-------------------------------------------------------------------*/
(function() {
    'use strict';

    // Toggle sidebar on Menu button click
    $('#sidebarCollapse').on('click', function() {
        $('#sidebar').toggleClass('active');
        $('#body').toggleClass('active');
    });

    // Auto-hide sidebar on window resize if window size is small
    // $(window).on('resize', function () {
    //     if ($(window).width() <= 768) {
    //         $('#sidebar, #body').addClass('active');
    //     }
    // });
})();

function togglePassword(inputId) {
  const passwordInput = document.querySelector(`#${inputId}`);
  const toggleIcon = document.querySelector(`#toggle${inputId}`);

  // �O�s�즳�����Ҫ��A
  const wasValid = passwordInput.classList.contains('input-validation-error');

  // Toggle the type attribute
  const type = passwordInput.getAttribute("type") === "password" ? "text" : "password";
  passwordInput.setAttribute("type", type);

  // Toggle the eye icon
  toggleIcon.classList.toggle('fa-eye');
  toggleIcon.classList.toggle('fa-eye-slash');

  // �p�G���e�O���Ī��A�����i��Q�[�W�����ҿ��~�˦�
  if (wasValid) {
    passwordInput.classList.remove('input-validation-error');
  }
}
