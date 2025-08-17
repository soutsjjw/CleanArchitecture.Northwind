window.initDeptOfficeLinkage = function (formSelector, officeList) {
    var $form = $(formSelector);
    var $department = $form.find('[name="DepartmentId"]');
    var $office = $form.find('[name="OfficeId"]');

    function renderOfficeOptions(deptId, selectedOfficeId) {
      $office.empty();
      $office.append('<option value="">請選擇</option>');
      if (deptId) {
        console.log('deptId:' + deptId);

        var filtered = officeList.filter(function (o) {
          return o.ParentValue == deptId;
        });
        filtered.forEach(function (o) {
            var selected = o.Value == selectedOfficeId ? 'selected="selected"' : '';
            $office.append('<option value="' + o.Value + '" ' + selected + '>' + o.Text + '</option>');
        });
      }
    }

    // 初始化（查詢後自動載入）
    var initDeptId = $department.val();
    var initOfficeId = $office.data('selected') || "";
    renderOfficeOptions(initDeptId, initOfficeId);

    // 部門變更時，重載單位
    $department.on('change', function () {
      console.log('Department changed to: ' + $(this).val());
      renderOfficeOptions($(this).val(), "");
    });
};