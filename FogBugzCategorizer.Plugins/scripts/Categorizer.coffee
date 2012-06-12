$ ->
	$('#Categorizer').click (e) ->
		e.preventDefault()

		$.ajax(
			type: 'GET',
			url: settings.url,
			dataType: 'xml',
			success: (xml) ->
				sel = $('<select />')
				$(xml).find('project').each ->
					project = $(this).attr('name')
					$('<option />', {value: project}).appendTo(sel)
				sel.appendTo '#CategorizerDiv'
		)
