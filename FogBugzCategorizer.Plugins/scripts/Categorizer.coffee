#Seems like the first line can't be real code, or at least a function, so using this first-line comment hack.
$ ->
	$.ajaxSetup(
		cache: false
	)

	$('#CategorizerDiv').hide()

	$('#Categorizer').click (e) ->
		e.preventDefault()
		$('#CategorizerDiv').toggle()
		if ($('#CategorizerDiv').is(':visible'))
			loadProjectsAndSelected()

	$('#CategorizerSave').click (e) ->
		e.preventDefault()
		$('#CategorizerNotifications').slideDown();
		$.ajax(
			type: 'POST',
			url: settings.url,
			data: JSON.stringify(
				Command: 'SaveCategories',
				BugzId: settings.bugzId,
				Categories: getSelectedCategories()
			),
			contentType: 'application/json; charset=utf-8',
			dataType: 'html',
			success: (result) ->
				$('#CategorizerNotifications').slideUp();
		)

	$('#TemplateSave').click (e) ->
		e.preventDefault()

		if ($('#newTemplateName').val() == '')
			alert('template name required!')
			return false

		$('#CategorizerNotifications').slideDown();

		$.ajax(
			type: 'POST',
			url: settings.url,
			data: JSON.stringify(
				Command: 'SaveTemplate',
				Name: $('#newTemplateName').val(),
				Categories: getSelectedCategories()
			),
			contentType: 'application/json; charset=utf-8',
			dataType: 'html',
			success: (result) ->
				$('#CategorizerNotifications').slideUp();
		)

loadProjectsAndSelected = (isTemplateChanged, templateName) ->
	if (isTemplateChanged)
		$('#CategorizerTasks').empty()
		$('#SelectedCategories').empty()
		$('#CategorizerDiv').data('loadAll', null)
	else
		data = $('#CategorizerDiv').data('loadAll')
		if (data && !isTemplateChanged)
			return
	$('#CategorizerNotifications').slideDown();
	$('#CategorizerProjects').empty()
	$.getJSON(settings.url,
		Command: 'LoadAll',
		BugzId: settings.bugzId,
		TemplateChanged: isTemplateChanged,
		TemplateName: templateName
	, (json) ->
		if (!isTemplateChanged)
			createTemplateDropdown($('#TemplateDropdownContainer'))

		$.each(json.Projects, (key, val) ->
			createProjectItem(val).appendTo('#CategorizerProjects')
		)
		$.each(json.Selected, (key, val) ->
			createSelectedItem(val).appendTo('#SelectedCategories')
		)

		if (!isTemplateChanged)
			$.each(json.Templates, (key, val) ->
				createTemplateItem(val)
			)

		$('#CategorizerDiv').data('loadAll', json)
		$('#CategorizerNotifications').slideUp();
	)

createTemplateDropdown = (container) ->
	$('<select id="selectedTemplate" name="selectedTemplate" style="display: block; visibility: visible;" />').appendTo(container).change (e) ->
		e.preventDefault()
		loadProjectsAndSelected(true, $(this).val())

createProjectItem = (project) ->
	div = $('<div />')
	div.html(project.Name).click (e) ->
		e.preventDefault()
		$('#CategorizerTasks').empty()
		data = $(this).data('tasks')
		if (data)
			createTasks(data)
		else
			$('#CategorizerNotifications').slideDown();
			projectObj = $(this)
			$.getJSON(settings.url,
				Command: 'GetTasks',
				Project: projectObj.html()
			, (json) ->
				createTasks(json, ->
					projectObj.data('tasks', json)
				)
				$('#CategorizerNotifications').slideUp();
			)
	div

createTasks = (data, func) ->
	$.each(data, (key, val) ->
		createTaskItem(val).appendTo('#CategorizerTasks')
	)
	if (func)
		func()

createTaskItem = (task) ->
	div = $('<div />')
	div.html(task.Name).click (e) ->
		e.preventDefault()
		if ($('#SelectedCategories div').filter( ->
			$(this).text() == getProjectTaskText(task) && $(this).is(':visible')
		).length < 1)
			addSelectedTask $(this), task
	div

addSelectedTask = (taskObj, task) ->
	createSelectedProjectTask taskObj, task
	taskObj.hide()

createSelectedItem = (task) ->
	div = $('<div />')
	div.data('task', task)
	div.html(getProjectTaskText(task)).click (e) ->
		e.preventDefault()
		$(this).hide()
	div.appendTo('#SelectedCategories')

createTemplateItem = (template) ->
	$('#selectedTemplate').append($('<option>',
		value: template.Name
	).text(template.Name).data('template', template))

createSelectedProjectTask = (taskObj, task) ->
	div = $('<div />')
	div.data('task', task)
	div.html(getProjectTaskText(task)).click (e) ->
		e.preventDefault()
		$(this).hide()
		taskObj.show()
	div.appendTo('#SelectedCategories')

getProjectTaskText = (task) ->
	return "#{task.Project.Name}: #{task.Name}"

getSelectedCategories = ->
	categories = []
	$('#SelectedCategories div').each ->
		if ($(this).is(':visible'))
			categories.push($(this).data('task'))
	return categories
