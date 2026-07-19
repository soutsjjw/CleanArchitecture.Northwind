window.initPagination = function (formSelector) {
  $(formSelector + ' .js-page').on('click', function () {
      var page = $(this).data('page');
      var $form = $(this).closest('form');
      if ($form.length === 0) $form = $('form[method="post"]'); // 外層查詢表單
      $form.find('input[name="pageNumber"]').remove();
      $form.append('<input type="hidden" name="pageNumber" value="' + page + '" />');
      $form.submit();
  });

  // 每頁筆數
  $(formSelector + ' .js-page-size').on('change', function () {
      var size = $(this).val();
      var $form = $(this).closest('form');
      if ($form.length === 0) $form = $('form[method="post"]');
      $form.find('input[name="pageSize"]').remove();
      $form.append('<input type="hidden" name="pageSize" value="' + size + '" />');
      $form.find('input[name="pageNumber"]').remove();
      $form.append('<input type="hidden" name="pageNumber" value="1" />');
      $form.submit();
  });
};