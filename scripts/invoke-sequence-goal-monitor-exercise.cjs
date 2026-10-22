const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const workflow = fs.readFileSync(path.join(__dirname, '../.github/workflows/ai-delivery-sequence-goal-associated-pr-monitor.yml'), 'utf8');
const start = workflow.indexOf('            const appSlug =');
const end = workflow.indexOf('            for (const candidate of candidates)');
assert(start >= 0 && end > start, 'Exercise must execute the actual discovery and document parsing code');
const code = workflow.slice(start, end).replace(/^            /gm, '');
const bindings = workflow.slice(workflow.indexOf('            const goalDigest ='), end).replace(/^            /gm, '');
const digestOf = vm.runInNewContext(bindings+';goalDigest', {require});
const finalizer = fs.readFileSync(path.join(__dirname, '../.github/workflows/ai-delivery-sequence-goal-pr-finalizer.yml'), 'utf8');
const finalBindings = finalizer.slice(finalizer.indexOf('            const goalDigest ='), finalizer.indexOf('            const aiResourceUsage =')).replace(/^            /gm, '');
const finalStart = finalizer.indexOf('                  if (!(await deliveryIsCurrent(');
const finalGate = finalizer.slice(finalStart, finalizer.indexOf('                  if (phase === "Production"', finalStart));
assert(finalStart >= 0 && finalGate.includes('throw new Error'), 'Finalization must reject stale bindings before writing Complete');
assert(finalizer.includes('Main changed during finalization; rerun to validate'), 'A main movement must rerun binding validation');
const bindingStart = workflow.indexOf('                let deliveryPending = true;');
const mergedGate = workflow.indexOf('                if (!pr.merged_at)', bindingStart);
assert(bindingStart > end && mergedGate > bindingStart, 'Every PR must be validated before the merged/open split');
const bindingGate = workflow.slice(bindingStart, mergedGate);
async function bindingPending(phase, merged, productionHead, qaHead, productionRecordHead, qaProductionHead, qaRecordHead, change='') {
  const requirements = '## Requirements\nLock account atomically.\n\n';
  const digest = digestOf(requirements);
  const production = {status:'Complete',verifiedCommit:productionRecordHead,identity:'context\n'+digest};
  const qa = {status:'Complete',productionCommit:qaProductionHead,verifiedCommit:qaProductionHead,verifiedQaCommit:qaRecordHead,pullRequest:'qa-url',goalDigest:digest};
  if (change==='production-digest') production.identity='old requirements';
  if (change==='qa-digest') qa.goalDigest='old requirements';
  if (change==='missing-digest') { delete production.identity; delete qa.goalDigest; }
  let goalText = requirements + [['Backend Production',production],['Backend QA',qa]].map(([task,state])=>'### Delivery: '+task+'\n```json\n'+JSON.stringify(state)+'\n```\n').join('\n');
  if (change==='requirements') goalText=goalText.replace('Lock account atomically.', 'Lock account and notify.');
  if (change==='evidence') goalText='| Harness Sensor | Added tests |\n| Acceptance evidence | Tests passed |\n'+goalText;
  const candidate = {goalText,associated:[{surface:'Backend',repository:'VAAD1ST/eapi',number:1}]};
  const item = {surface:phase==='QA'?'QA-Backend':'Backend'};
  const pr = {head:{sha:phase==='QA'?qaHead:productionHead},html_url:'qa-url',merged_at:merged?'today':null};
  const sandbox = {require,candidate,item,pr,github:{rest:{pulls:{get:async()=>({data:{head:{sha:productionHead}}})}}},core:{info(){},warning(){} }};
  const pending = await vm.runInNewContext('(async()=>{'+bindings+';let hasDeliveryPending=false;'+bindingGate+';return hasDeliveryPending;})()', sandbox);
  const rejected = await vm.runInNewContext('(async()=>{'+finalBindings+';try {'+finalGate+';return false;} catch(error) { if (!error.message.includes("verification does not match")) throw error; return true; }})()', {...sandbox,text:goalText,childRows:candidate.associated,row:item,childPr:pr,diagram:'test.md'});
  assert.equal(rejected,pending,'Monitor and finalization must enforce the same bindings');
  return pending;
}
const goal = '| Status | `InProgress` |\n\n## Open points to finalize\n\n- None.\n\n## Delivery progress\n\n### Delivery: Backend Production\n```json\n{"status":"InProgress","branch":"production"}\n```\n';
async function exercise(status, author, text, children=null, moved=false, options={}) {
  const pr = { number: 1, node_id: 'PR1', state: status, draft: true, user: {login: author}, head: {ref:'goal',sha:'b'.repeat(40),repo:{full_name:'VAAD1ST/eapi'}}, base:{ref:'main'}, body:'Automated PR raised by the sequence goal loop after implementation verification.' };
  let ready = 0; const dispatches = []; const warnings = [];
  let listCalls = 0;
  const github = {rest:{pulls:{list:'prs',listFiles:'files',listReviewComments:'empty',listReviews:'empty',get:async({pull_number})=>({data:children.find(child=>child.number===pull_number)})},repos:{listBranches:'branches',getContent:async({ref})=>{assert.equal(ref,'b'.repeat(40),'Goal read must be pinned to the inspected coordinator commit');return {data:{content:Buffer.from(text).toString('base64')}};}}},graphql:async()=>{ready++;return {markPullRequestReadyForReview:{pullRequest:{isDraft:false}}};}};
  github.rest.issues = {listComments:'empty'};
  github.rest.checks = {listForRef:'empty'};
  github.rest.repos.getCombinedStatusForRef = async()=>({data:{statuses:[]}});
  github.rest.actions = {
    listWorkflowRuns:async({workflow_id,status})=> {
      if (options.lookupFails) throw new Error('unavailable');
      return {data:{workflow_runs:options.busyWorkflow===workflow_id && status==='in_progress' ? [{status}] : []}};
    },
    createWorkflowDispatch:async(args)=>dispatches.push(args)
  };
  github.paginate = async (method, args) => {
    if (method === 'empty') return [];
    if (method === 'prs') {
      listCalls++;
      const current = moved && listCalls > 1 ? {...pr,head:{...pr.head,sha:'d'.repeat(40)}} : pr;
      return args.state === 'all' || args.state === pr.state ? [current] : [];
    }
    if (method === 'files') return [{filename:'docs/sequence-diagrams/test.md',status:'modified'}];
    throw new Error('Unexpected API call: '+method);
  };
  github.paginate.iterator = async function* () { yield {data:[{name:'main',commit:{sha:'a'.repeat(40)}},{name:'goal',commit:{sha:'b'.repeat(40)}}]}; };
  const executed = children === null ? code : workflow.slice(start).replace(/^            /gm, '');
  const result = await vm.runInNewContext('(async()=>{'+executed+';return candidates;})()', {require,github,context:{repo:{owner:'VAAD1ST',repo:'eapi'}},process:{env:{SEQUENCE_GOAL_COORDINATOR_REPOSITORY:'VAAD1ST/eapi'}},Buffer,core:{info(){},warning(message){warnings.push(message);}}});
  return {result,ready,dispatches,warnings};
}
(async()=>{
  const samples = ['## Requirements\nLock.', '# Goal\n\n## Agent Loop Goal\n| Goal | Lock |\n| Done when | Tested |\n| Status | Complete |\n| Harness Sensor | Tests |\n\n## Requirements\nLock.\n\n## Delivery progress\n### Delivery: Backend QA\n```json\n{"status":"Complete"}\n```\n'];
  samples.push(samples[1].replace(/\n/g,'\r\n'));
  const ps = require('node:child_process').execFileSync('pwsh',['-NoProfile','-Command','. $env:GOAL_LIBRARY; @($env:GOAL_SAMPLES | ConvertFrom-Json) | ForEach-Object { Get-ProductionGoalDigest $_ }'],{encoding:'utf8',env:{...process.env,GOAL_LIBRARY:path.join(__dirname,'sequence-goal-production-readiness.ps1'),GOAL_SAMPLES:JSON.stringify(samples)}}).trim().split(/\r?\n/);
  assert.deepEqual(samples.map(digestOf),ps,'JavaScript readiness must match the production PowerShell digest');
  const finalDigest = vm.runInNewContext(finalBindings+';goalDigest',{require});
  assert.deepEqual(samples.map(finalDigest),ps,'Finalizer digest must match production verification');
  const p='a'.repeat(40), q='b'.repeat(40), changed='c'.repeat(40);
  for (const merged of [false,true]) {
    assert.equal(await bindingPending('Production',merged,p,q,p,p,q),false);
    assert.equal(await bindingPending('Production',merged,changed,q,p,p,q),true);
    assert.equal(await bindingPending('QA',merged,p,q,p,p,q),false);
    assert.equal(await bindingPending('QA',merged,changed,q,changed,p,q),true);
    assert.equal(await bindingPending('QA',merged,p,changed,p,p,q),true);
    assert.equal(await bindingPending('QA',merged,p,q,changed,p,q),true);
    for (const phase of ['Production','QA']) {
      for (const change of ['requirements','production-digest','missing-digest']) assert.equal(await bindingPending(phase,merged,p,q,p,p,q,change),true);
      assert.equal(await bindingPending(phase,merged,p,q,p,p,q,'evidence'),false);
    }
    assert.equal(await bindingPending('QA',merged,p,q,p,p,q,'qa-digest'),true);
  }
  const active = await exercise('open','github-actions[bot]',goal);
  assert.equal(active.result.length,1);
  assert.equal(active.result[0].goalText,goal, 'The goal document must remain available when evaluating task readiness');
  assert.equal((await exercise('closed','github-actions[bot]',goal)).result.length,0);
  assert.equal((await exercise('open','untrusted-user',goal)).result.length,0);
  assert.equal((await exercise('open','github-actions[bot]',goal+'\n| Final state | `Abandoned` |\n')).result.length,0);
  const complete = await exercise('open','github-actions[bot]',goal.replace('`InProgress`','`Complete`'));
  assert.equal(complete.ready,0,'Discovery must never publish readiness');
  assert.equal(complete.result.length,1);
  assert.equal(complete.result[0].recoverComplete,true);
  const baseGoal = '# Goal\n\n## Agent Loop Goal\n| Status | `Complete` |\n| Goal | Protect account |\n\n## Requirements\nLock atomically.\n\n## Open points to finalize\n\n- None.\n\n## Associated delivery pull requests\n| `Backend` | `VAAD1ST/eapi` | `main` | `production` | [#2](production-url) |\n| `QA-Backend` | `VAAD1ST/eapi` | `main` | `qa` | [#3](qa-url) |\n';
  for (const scenario of ['current','requirements','production-head','qa-head','qa-binding','missing-state','coordinator-moved']) {
    const digest = digestOf(baseGoal);
    const states = [['Backend Production',{status:'Complete',verifiedCommit:p,identity:digest}],['Backend QA',{status:'Complete',productionCommit:scenario==='qa-binding'?changed:p,verifiedCommit:p,verifiedQaCommit:q,pullRequest:'qa-url',goalDigest:digest}]];
    let text = baseGoal + '\n## Delivery progress\n' + (scenario==='missing-state'?'':states.map(([task,state])=>'### Delivery: '+task+'\n```json\n'+JSON.stringify(state)+'\n```\n').join('\n'));
    if (scenario==='requirements') text=text.replace('Lock atomically.','Lock and notify.');
    const children = [['Production',2,'production',scenario==='production-head'?changed:p],['QA',3,'qa',scenario==='qa-head'?changed:q]].map(([phase,number,branch,sha])=>({number,html_url:branch+'-url',state:'closed',merged_at:'today',head:{sha,ref:branch,repo:{full_name:'VAAD1ST/eapi'}},base:{ref:'main'},user:{login:'github-actions[bot]'},body:'Associated sequence-goal delivery for `VAAD1ST/eapi@goal`.\n- Phase: `'+phase+'`\n- Surface: `Backend`'}));
    const run = await exercise('open','github-actions[bot]',text,children,scenario==='coordinator-moved');
    assert.equal(run.ready,scenario==='current'?1:0,scenario+' Complete goal readiness');
  }
  // Execute the complete monitor against each lifecycle transition.
  for (const scenario of ['no-children','incomplete','missing-qa','qa-incomplete','waiting-review','merged','completion-exhausted','blocked','exhausted','production-busy','qa-busy','lookup-failed','coordinator-moved']) {
    let text = baseGoal.replace('`Complete`','`InProgress`');
    const digest=digestOf(text);
    let children=[['Production',2,'production',p],['QA',3,'qa',q]].map(([phase,number,branch,sha])=>({number,html_url:branch+'-url',state:'open',draft:false,merged_at:null,head:{sha,ref:branch,repo:{full_name:'VAAD1ST/eapi'}},base:{ref:'main'},user:{login:'github-actions[bot]'},body:'Associated sequence-goal delivery for `VAAD1ST/eapi@goal`.\n- Phase: `'+phase+'`\n- Surface: `Backend`'}));
    const production={status:'Complete',verifiedCommit:p,identity:digest};
    const qa={status:'Complete',productionCommit:p,verifiedCommit:p,verifiedQaCommit:q,pullRequest:'qa-url',goalDigest:digest};
    if (scenario==='no-children') { children=[]; text=text.replace(/^\| `(Backend|QA-Backend)`.*\n/gm,''); }
    if (scenario==='missing-qa') { children=children.slice(0,1); text=text.replace(/^\| `QA-Backend`.*\n/gm,''); }
    if (['incomplete','production-busy','qa-busy','lookup-failed','coordinator-moved','exhausted'].includes(scenario)) production.status='InProgress';
    if (scenario==='blocked') production.status='Blocked';
    if (scenario==='qa-incomplete') qa.status='InProgress';
    if (['merged','completion-exhausted'].includes(scenario)) for (const child of children) { child.state='closed'; child.merged_at='today'; }
    text+='\n## Delivery progress\n'+[['Backend Production',production],['Backend QA',qa],['Coordinator',{automaticContinuations:['exhausted','completion-exhausted'].includes(scenario)?3:0,continuationStage:scenario==='completion-exhausted'?'Completion':'Delivery'}]].map(([task,state])=>'### Delivery: '+task+'\n```json\n'+JSON.stringify(state)+'\n```\n').join('\n');
    const options={lookupFails:scenario==='lookup-failed',busyWorkflow:scenario==='production-busy'?'ai-delivery-sequence-goal-loop.yml':scenario==='qa-busy'?'ai-delivery-sequence-goal-qa.yml':null};
    const run=await exercise('open','github-actions[bot]',text,children,scenario==='coordinator-moved',options);
    const shouldDispatch=['no-children','incomplete','missing-qa','qa-incomplete','merged'].includes(scenario);
    assert.equal(run.dispatches.length,shouldDispatch?1:0,scenario+' dispatch decision');
    assert.equal(run.ready,0,scenario+' must retain draft coordinator');
    if (shouldDispatch) assert.equal(run.dispatches[0].inputs.automatic_resume,'true',scenario+' recovery budget');
  }
  console.log('Monitor exercise passed: document discovery, task context, untrusted/closed/abandoned exclusion, Complete publication.');
})().catch(error=>{console.error(error);process.exitCode=1;});
