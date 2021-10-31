$(".reveal").on('click',function() {
		var $pwd = $(".pwd");
		var $bi = $(".bi");
		if ($pwd.attr('type') === 'password') {
			$pwd.attr('type', 'text');
			$bi.removeClass('bi-eye-slash');
			$bi.addClass('bi-eye');
			
		} else {
			$pwd.attr('type', 'password');
			$bi.removeClass('bi-eye');
			$bi.addClass('bi-eye-slash');
		}
	});