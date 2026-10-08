import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';

const projectRoot = path.resolve(import.meta.dirname, '../../..');

test('antigravity skill: structure, frontmatter and discovery', async () => {
  const skillPath = path.join(projectRoot, '.agents', 'skills', 'game-development', 'SKILL.md');
  const content = await fs.readFile(skillPath, 'utf8');

  // Verify YAML frontmatter
  assert.match(content, /^---\r?\n/);
  assert.match(content, /name:\s*game-development/);
  assert.match(content, /description:/);

  // Verify Claude-specific tool assumptions are eliminated
  assert.doesNotMatch(content, /allowed-tools:/);
  assert.doesNotMatch(content, /~[\\\/]\.claude/);

  // Verify Antigravity tools & workflow are used
  assert.match(content, /view_file/);
  assert.match(content, /replace_file_content/);
  assert.match(content, /write_to_file/);
});

test('antigravity skill: unity 2D and core principles guidance', async () => {
  const skillPath = path.join(projectRoot, '.agents', 'skills', 'game-development', 'SKILL.md');
  const content = await fs.readFile(skillPath, 'utf8');

  // Core principles
  assert.match(content, /FixedUpdate/);
  assert.match(content, /The Game Loop/);
  assert.match(content, /State Machine/);
  assert.match(content, /Object Pooling/);

  // Unity 2D specific
  assert.match(content, /Rigidbody2D/);
  assert.match(content, /FreezeRotation/);
  assert.match(content, /Collider2D/);
  assert.match(content, /linearVelocity/);
  assert.match(content, /Batchmode/i);
});

test('antigravity skill: project memory integration and progressive disclosure', async () => {
  const skillPath = path.join(projectRoot, '.agents', 'skills', 'game-development', 'SKILL.md');
  const content = await fs.readFile(skillPath, 'utf8');

  // Project Memory integration
  assert.match(content, /PROJECT_STATE\.md/);
  assert.match(content, /get_project_context/);
  assert.match(content, /report_result/);

  // Progressive disclosure references
  const refDir = path.join(projectRoot, '.agents', 'skills', 'game-development', 'references');
  const files = await fs.readdir(refDir);
  assert.ok(files.includes('unity-2d.md'));
  assert.ok(files.includes('patterns.md'));
  assert.ok(files.includes('architecture-pho-nho.md'));
  assert.ok(files.includes('routing.md'));

  for (const file of ['unity-2d.md', 'patterns.md', 'architecture-pho-nho.md', 'routing.md']) {
    const refContent = await fs.readFile(path.join(refDir, file), 'utf8');
    assert.ok(refContent.length > 200, `Reference ${file} should contain substantive content`);
  }
});

test('antigravity skill: attribution and source licensing', async () => {
  const skillPath = path.join(projectRoot, '.agents', 'skills', 'game-development', 'SKILL.md');
  const content = await fs.readFile(skillPath, 'utf8');

  assert.match(content, /davila7\/claude-code-templates/);
  assert.match(content, /MIT License/);
  assert.match(content, /Daniel Vila Suero/);
});

test('antigravity skill: workspace copy sync in skills/ directory', async () => {
  const agentSkillPath = path.join(projectRoot, '.agents', 'skills', 'game-development', 'SKILL.md');
  const workspaceSkillPath = path.join(projectRoot, 'skills', 'game-development', 'SKILL.md');

  const agentContent = await fs.readFile(agentSkillPath, 'utf8');
  const workspaceContent = await fs.readFile(workspaceSkillPath, 'utf8');

  assert.equal(agentContent, workspaceContent);
});
