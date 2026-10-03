# Convertit un .usda (Quaternius / Blender) en données de maillage simples pour Unity (JSON).
import re, json, math, sys

def parse_floats(s): return [float(x) for x in re.findall(r'-?\d+\.?\d*(?:e[-+]?\d+)?', s)]

def mat_mul(a, b): return [[sum(a[i][k]*b[k][j] for k in range(4)) for j in range(4)] for i in range(4)]
def ident(): return [[1 if i==j else 0 for j in range(4)] for i in range(4)]
def rx(d):
    c,s=math.cos(math.radians(d)),math.sin(math.radians(d)); return [[1,0,0,0],[0,c,-s,0],[0,s,c,0],[0,0,0,1]]
def ry(d):
    c,s=math.cos(math.radians(d)),math.sin(math.radians(d)); return [[c,0,s,0],[0,1,0,0],[-s,0,c,0],[0,0,0,1]]
def rz(d):
    c,s=math.cos(math.radians(d)),math.sin(math.radians(d)); return [[c,-s,0,0],[s,c,0,0],[0,0,1,0],[0,0,0,1]]

def convert(path):
    lines = open(path, encoding='utf-8').read().split('\n')
    mats = {}
    stack = []   # [name, kind, ops dict, order, depth]
    depth = 0
    meshes = []
    cur = None
    pending = None
    curmat = None
    for ln in lines:
        st = ln.strip()
        m = re.match(r'def (\w+) "([^"]+)"', st)
        if m:
            pending = {'kind': m.group(1), 'name': m.group(2), 'ops': {}, 'order': [], 'data': {}}
            if m.group(1) == 'Material': curmat = m.group(2)
        if st.startswith('{') and pending is not None:
            depth += 1
            pending['depth'] = depth
            stack.append(pending)
            pending = None
            continue
        if st.startswith('{'): depth += 1; continue
        if st.startswith('}'):
            if stack and stack[-1].get('depth') == depth:
                node = stack.pop()
                if node['kind'] == 'Mesh': meshes.append((node, [dict(n) for n in stack]))
                if node['kind'] == 'GeomSubset' and stack:
                    stack[-1]['data'].setdefault('subsets', []).append((node['data'].get('sub', []), node['data'].get('mat')))
            depth -= 1
            continue
        if not stack: continue
        node = stack[-1]
        if 'xformOp:' in st and '=' in st and 'xformOpOrder' not in st:
            k = re.search(r'xformOp:(\w+)', st).group(1)
            node['ops'][k] = parse_floats(st.split('=', 1)[1])
        elif 'xformOpOrder' in st:
            node['order'] = re.findall(r'xformOp:(\w+)', st)
        elif st.startswith('point3f[] points'):
            node['data']['points'] = parse_floats(st.split('=', 1)[1])
        elif st.startswith('int[] faceVertexCounts'):
            node['data']['counts'] = [int(x) for x in parse_floats(st.split('=', 1)[1])]
        elif st.startswith('int[] faceVertexIndices'):
            node['data']['indices'] = [int(x) for x in parse_floats(st.split('=', 1)[1])]
        elif st.startswith('int[] indices') and node['kind'] == 'GeomSubset':
            node['data']['sub'] = [int(x) for x in parse_floats(st.split('=', 1)[1])]
        elif st.startswith('rel material:binding'):
            node['data']['mat'] = st.split('/')[-1].rstrip('>')
        elif 'inputs:diffuseColor' in st and curmat:
            mats[curmat] = parse_floats(st.split('=', 1)[1])[:3]

    def xf(node):
        M = ident()
        for op in node['order']:
            v = node['ops'].get(op)
            if v is None: continue
            if op == 'translate': T = ident(); T[0][3], T[1][3], T[2][3] = v[:3]; M = mat_mul(M, T)
            elif op == 'scale': S = ident(); S[0][0], S[1][1], S[2][2] = v[:3]; M = mat_mul(M, S)
            elif op == 'rotateXYZ': M = mat_mul(M, mat_mul(rz(v[2]), mat_mul(ry(v[1]), rx(v[0]))))
        return M

    parts = {}
    for node, parents in meshes:
        d = node['data']
        if 'points' not in d: continue
        M = ident()
        for p in parents: M = mat_mul(M, xf(p))
        M = mat_mul(M, xf(node))
        pts = d['points']; P = []
        for i in range(0, len(pts), 3):
            x, y, z = pts[i:i+3]
            X = M[0][0]*x + M[0][1]*y + M[0][2]*z + M[0][3]
            Y = M[1][0]*x + M[1][1]*y + M[1][2]*z + M[1][3]
            Z = M[2][0]*x + M[2][1]*y + M[2][2]*z + M[2][3]
            P.append((-X, Y, Z))   # main gauche Unity
        faceMat = {}
        for idx, mname in d.get('subsets', []):
            for fi in idx: faceMat[fi] = mname
        bases = {}
        k = 0
        for fi, n in enumerate(d['counts']):
            f = d['indices'][k:k+n]; k += n
            key = faceMat.get(fi, d.get('mat', 'default'))
            part = parts.setdefault(key, {'color': [round(c, 4) for c in mats.get(key, [0.6, 0.6, 0.6])], 'v': [], 't': []})
            if (key, id(node)) not in bases:
                bases[(key, id(node))] = len(part['v']) // 3
                for p in P: part['v'] += [round(c, 5) for c in p]
            base = bases[(key, id(node))]
            for j in range(1, n - 1):
                part['t'] += [base + f[0], base + f[j + 1], base + f[j]]   # ordre inversé (main gauche)
    return {'parts': list(parts.values())}

if __name__ == '__main__':
    src, dst = sys.argv[1], sys.argv[2]
    data = convert(src)
    allv = [c for p in data['parts'] for c in p['v']]
    xs, ys, zs = allv[0::3], allv[1::3], allv[2::3]
    print(src, len(data['parts']), 'parts', len(allv)//3, 'verts', 'size', round(max(xs)-min(xs),3), round(max(ys)-min(ys),3), round(max(zs)-min(zs),3), [p['color'] for p in data['parts']])
    json.dump(data, open(dst, 'w'), separators=(',', ':'))
