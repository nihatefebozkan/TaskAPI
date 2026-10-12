from pathlib import Path
from html import escape
from docx import Document
from docx.table import Table
from docx.text.paragraph import Paragraph

root=Path(__file__).resolve().parent
doc=Document(root/'TaskAPI_Auth_Calisma_Kitabi.docx')
parts=[]
for el in doc.element.body:
    if el.tag.endswith('}p'):
        par=Paragraph(el,doc)
        text=par.text
        if not text.strip(): continue
        st=par.style.name
        level={'Title':'h1','Heading 1':'h2','Heading 2':'h3'}.get(st,'p')
        if st=='Heading 1':
            ident='section-'+str(sum(x.startswith('<h2') for x in parts)+1)
            parts.append(f'<h2 id="{ident}">{escape(text)}</h2>')
        elif par.runs and par.runs[0].font.name=='Consolas':
            parts.append('<pre><code>'+escape(text)+'</code></pre>')
        elif text.startswith('https://'):
            parts.append(f'<p class="source"><a href="{escape(text)}">{escape(text)}</a></p>')
        else: parts.append(f'<{level}>'+escape(text)+f'</{level}>')
    elif el.tag.endswith('}tbl'):
        tb=Table(el,doc)
        rows=[]
        for i,row in enumerate(tb.rows):
            tag='th' if i==0 else 'td'
            rows.append('<tr>'+''.join(f'<{tag}>{escape(c.text)}</{tag}>' for c in row.cells)+'</tr>')
        parts.append('<div class="table-wrap"><table>'+''.join(rows)+'</table></div>')
nav=''.join(f'<a href="#section-{i}">Faz {i}</a>' for i in range(1,9))
html='''<!doctype html><html lang="tr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>TaskAPI Auth Çalışma Kitabı</title><style>
*{box-sizing:border-box}body{margin:0;background:#f4f5f7;color:#20242b;font:17px/1.65 system-ui,Segoe UI,sans-serif}main{max-width:900px;margin:36px auto;background:white;padding:48px 60px;border-radius:12px}h1{font-size:36px;line-height:1.2;color:black}h2{font-size:29px;line-height:1.3;margin:64px 0 24px;border-top:1px solid #ddd;padding-top:30px;scroll-margin-top:20px}h3{font-size:20px;color:black;margin:28px 0 8px}p{margin:10px 0 16px}pre{font-size:14px;line-height:1.6;background:#f1f3f5;padding:20px;overflow-x:auto;border-radius:6px}table{width:100%;border-collapse:collapse;font-size:15px}th,td{text-align:left;border:1px solid #d9d9d9;padding:12px 14px}th{background:#dce6f1;color:black}.table-wrap{overflow-x:auto;margin:20px 0}nav{display:flex;flex-wrap:wrap;gap:8px;margin:24px 0}nav a{padding:6px 12px;background:#eaf0f6;border-radius:4px;color:#234b73;text-decoration:none}.source{font-size:13px;overflow-wrap:anywhere}a{color:#234b73}@media(max-width:650px){main{margin:0;padding:24px}h1{font-size:29px}}@media print{body{background:white;font-size:11pt}main{margin:0;padding:0;max-width:none}nav{display:none}h2{break-before:page;border:0;padding:0;margin-top:0}h3{break-after:avoid}table,pre{break-inside:avoid}}
</style><main>'''+nav+''.join(parts)+'</main></html>'
out=root/'TaskAPI_Auth_Calisma_Kitabi.html'
out.write_text(html,encoding='utf-8')
print(out)
