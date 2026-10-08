export function actionSchema(url) {
  const stringArray = (maxItems, maxLength) => ({ type: 'array', maxItems, items: { type: 'string', maxLength } });
  const taskInput = { type: 'object', additionalProperties: false,
    required: ['id', 'title', 'goal', 'files', 'steps', 'acceptance_criteria'],
    properties: { id: { type: 'string', pattern: '^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$', description: 'Stable ID; reuse only when retrying the exact same task.' },
      title: { type: 'string', maxLength: 160 }, goal: { type: 'string', maxLength: 10000 },
      files: stringArray(50, 240), steps: stringArray(40, 1500), acceptance_criteria: stringArray(40, 1000) } };
  const task = { type: 'object', properties: { ...taskInput.properties,
    status: { type: 'string', enum: ['pending', 'in_progress', 'blocked', 'done'] }, revision: { type: 'integer' },
    worker: { type: ['string', 'null'] }, result: { type: ['object', 'null'], properties: {
      summary: { type: 'string' }, changed_files: stringArray(100, 240), validation: stringArray(40, 1000), commit: { type: ['string', 'null'] } } } } };
  const response = (schema, description = 'Success') => ({ description, content: { 'application/json': { schema } } });
  const envelope = { type: 'object', properties: { task: { $ref: '#/components/schemas/Task' } } };
  const errors = { '400': response({ type: 'object', properties: { error: { type: 'string' } } }, 'Invalid task'),
    '401': { description: 'Missing or invalid bearer token' }, '409': { description: 'ID conflict or queue busy' }, '429': { description: 'Rate limit or queue limit' } };
  return { openapi: '3.1.0', info: { title: 'Pho Nho IDE Task Bridge', version: '0.1.0', description: 'Send user-approved task descriptions to a local project queue and read IDE results.' },
    servers: [{ url }], security: [{ bearerAuth: [] }],
    paths: {
      '/health': { get: { operationId: 'getBridgeHealth', summary: 'Check the project bridge connection', responses: { '200': response({ type: 'object', properties: { ok: { type: 'boolean' }, version: { type: 'string' } } }), '401': errors['401'] } } },
      '/v1/tasks': {
        get: { operationId: 'listProjectTasks', summary: 'List task status summaries', parameters: [
          { name: 'status', in: 'query', required: false, schema: { type: 'string', enum: ['pending', 'in_progress', 'blocked', 'done'] } },
          { name: 'offset', in: 'query', required: false, schema: { type: 'integer', minimum: 0 } }], responses: { '200': response({ type: 'object', properties: {
            tasks: { type: 'array', items: { type: 'object', properties: { id: { type: 'string' }, title: { type: 'string' }, status: { type: 'string' }, revision: { type: 'integer' }, result_summary: { type: ['string', 'null'] } } } },
            next_offset: { type: ['integer', 'null'] } } }), ...errors } },
        post: { operationId: 'submitProjectTask', summary: 'Save an approved task for the IDE agent',
          description: 'Stores task text in the project queue. Does not execute commands, change project code, or start the IDE agent.',
          'x-openai-isConsequential': false,
          requestBody: { required: true, content: { 'application/json': { schema: { $ref: '#/components/schemas/TaskInput' } } } },
          responses: { '201': response(envelope, 'Task created'), '200': response(envelope, 'Exact retry: existing task'), ...errors } }
      },
      '/v1/context': {
        get: { operationId: 'getProjectContext', summary: 'Get condensed project memory and active tasks in 1 call',
          parameters: [
            { name: 'include_pending', in: 'query', required: false, schema: { type: 'boolean' } },
            { name: 'include_in_progress', in: 'query', required: false, schema: { type: 'boolean' } }
          ],
          responses: {
            '200': response({
              type: 'object',
              properties: {
                project_memory: { type: 'string' },
                active_tasks: { type: 'array', items: { type: 'object' } },
                stats: { type: 'object' }
              }
            }), ...errors
          }
        }
      },
      '/v1/tasks/{id}': { get: { operationId: 'getProjectTask', summary: 'Read a task and the IDE validation result',
        parameters: [{ name: 'id', in: 'path', required: true, schema: { type: 'string', pattern: '^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$' } }],
        responses: { '200': response(envelope), '404': { description: 'Task not found' }, ...errors } } }
    }, components: { securitySchemes: { bearerAuth: { type: 'http', scheme: 'bearer' } }, schemas: { TaskInput: taskInput, Task: task } } };
}
