$ ->
	$.ajaxSetup(
		cache: false
	)

	console.log "jQuery version: #{$().jquery}"

	$('#CategorizerDiv').hide()

	$('#Categorizer').click (e) ->
		e.preventDefault()
		$('#CategorizerDiv').toggle()
		if ($('#CategorizerDiv').is(':visible'))
			createProjects()

	$('#CategorizerSave').click (e) ->
		e.preventDefault()
		$.ajax(
			type: 'POST',
			url: settings.url,
			data: JSON.stringify(
				Command: 'SaveCategories'
				Categories: getSelectedCategories()
			),
			contentType: "application/json; charset=utf-8",
			dataType: 'json',
			success: (result) ->
				console.log "finished saving categories, got result: #{result}"
		)

createProjects = ->
	data = $('#CategorizerDiv').data('projects')
	if (data)
		return
	$('#CategorizerProjects').empty()
	$.getJSON(settings.url,
		Command: 'GetProjects'
	, (json) ->
		$.each(json, (key, val) ->
			createProjectItem(val).appendTo('#CategorizerProjects')
		)
		$('#CategorizerDiv').data('projects', json)
	)

createProjectItem = (project) ->
	div = $('<div />')
	div.html(project.Name).click (e) ->
		e.preventDefault()
		$('#CategorizerTasks').empty()
		data = $(this).data('tasks')
		if (data)
			createTasks(data)
		else
			projectObj = $(this)
			$.getJSON(settings.url,
				Command: 'GetTasks',
				Project: projectObj.html()
			, (json) ->
				createTasks(json, ->
					projectObj.data('tasks', json)
				)
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
		categories.push($(this).data('task'))
	return categories
