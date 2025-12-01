# My Workflow Instructions

<critical>The workflow execution engine is governed: {project-root}/.bmad/core/tasks/workflow.xml</critical>
<critical>You MUST have already loaded and processed: {project-root}/.bmad/custom/workflows/my-workflow/workflow.yaml</critical>
<critical>Communicate in {communication_language} throughout the workflow process</critical>

<workflow>

<step n="1" goal="Understand the workflow purpose">
<action>Greet {user_name} and explain the purpose of this workflow</action>
<action>Gather any initial requirements or context needed</action>
</step>

<step n="2" goal="Execute main workflow logic">
<action>Perform the main workflow tasks</action>
<action>Collect any necessary information from the user</action>
<template-output>workflow_results</template-output>
</step>

<step n="3" goal="Generate output">
<action>Create the output document using the template</action>
<action>Save to {default_output_file}</action>
</step>

</workflow>

