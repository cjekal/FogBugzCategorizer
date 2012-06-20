#for some reason, CoffeeScript hates having this "unless console" block as the first line... causes IDENT error on line 2 where i set the console object so instead resorting to this hack of having the 1st line as a comment.
unless console
	console =
		messages: []
		log: (msg) ->
			@messages.push(msg)

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
			contentType: "application/json; charset=utf-8",
			dataType: 'html',
			success: (result) ->
				console.log "finished saving categories, got result: #{result}"
				$('#CategorizerNotifications').slideUp();
		)

loadProjectsAndSelected = ->
	data = $('#CategorizerDiv').data('loadAll')
	if (data)
		return
	$('#CategorizerNotifications').slideDown();
	$('#CategorizerProjects').empty()
	$.getJSON(settings.url,
		Command: 'LoadAll',
		BugzId: settings.bugzId
	, (json) ->
		console.log "finished getting LoadAll, got result: #{JSON.stringify(json)}"
		$.each(json.Projects, (key, val) ->
			createProjectItem(val).appendTo('#CategorizerProjects')
		)
		$.each(json.Selected, (key, val) ->
			createSelectedItem(val).appendTo('#SelectedCategories')
		)
		$('#CategorizerDiv').data('loadAll', json)
		$('#CategorizerNotifications').slideUp();
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
			$('#CategorizerNotifications').slideDown();
			projectObj = $(this)
			$.getJSON(settings.url,
				Command: 'GetTasks',
				Project: projectObj.html()
			, (json) ->
				console.log "finished getting GetTasks, got result: #{JSON.stringify(json)}"
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
