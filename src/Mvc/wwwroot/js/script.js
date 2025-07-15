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

  // 保存原有的驗證狀態
  const wasValid = passwordInput.classList.contains('input-validation-error');

  // Toggle the type attribute
  const type = passwordInput.getAttribute("type") === "password" ? "text" : "password";
  passwordInput.setAttribute("type", type);

  // Toggle the eye icon
  toggleIcon.classList.toggle('fa-eye');
  toggleIcon.classList.toggle('fa-eye-slash');

  // 如果之前是有效的，移除可能被加上的驗證錯誤樣式
  if (wasValid) {
    passwordInput.classList.remove('input-validation-error');
  }
}
