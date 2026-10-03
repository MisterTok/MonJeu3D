import glob,os,sys,json,math
from PIL import Image
A=os.path.expanduser('~/mnt/Assets')
OUT=os.path.expanduser('~/mnt/MonJeu3D/Assets/Resources/ForgeSprites')
def srcdir(name):
    if name.startswith('Skeleton_Crusader'):
        return A+'/sprites/character/skeleton_crusader/'+name+'/PNG/PNG Sequences'
    return A+'/_Project/Art/Characters/'+name
WANT={'idle':['Idle'],'move':['Walking'],'attack':['Slashing','Throwing'],'death':['Dying']}
TARGET_H=240
def process(name):
    d=srcdir(name); anims={}
    for k,opts in WANT.items():
        for o in opts:
            fs=sorted(glob.glob(d+'/'+o+'/*.png'))
            if fs: anims[k]=fs; break
    ims={k:[Image.open(f).convert('RGBA') for f in fs] for k,fs in anims.items()}
    def bbox(lst):
        b=None
        for im in lst:
            bb=im.getbbox()
            if bb: b=bb if b is None else (min(b[0],bb[0]),min(b[1],bb[1]),max(b[2],bb[2]),max(b[3],bb[3]))
        return b
    ub=bbox([i for l in ims.values() for i in l]); ib=bbox(ims['idle'])
    s=TARGET_H/(ib[3]-ib[1])
    cw=math.ceil((ub[2]-ub[0])*s)+2; ch=math.ceil((ub[3]-ub[1])*s)+2
    os.makedirs(OUT+'/'+name,exist_ok=True)
    meta={'cellW':cw,'cellH':ch,'pivotX':((ib[0]+ib[2])/2-ub[0])*s+1,'pivotY':(ub[3]-ib[3])*s+1,
          'idleW':(ib[2]-ib[0])*s,'idleH':TARGET_H,'anims':[]}
    for k,lst in ims.items():
        n=len(lst); cols=max(1,min(n,2048//cw,math.ceil(math.sqrt(n*ch/cw))))
        rows=math.ceil(n/cols)
        sheet=Image.new('RGBA',(cols*cw,rows*ch),(0,0,0,0))
        for i,im in enumerate(lst):
            c=im.crop(ub).resize((cw-2,ch-2),Image.LANCZOS)
            sheet.paste(c,((i%cols)*cw+1,(i//cols)*ch+1))
        sheet.save(OUT+'/'+name+'/'+k+'.png',optimize=True)
        meta['anims'].append({'name':k,'frames':n,'cols':cols})
    json.dump(meta,open(OUT+'/'+name+'/meta.json','w'))
    print(name,cw,ch,{a['name']:a['frames'] for a in meta['anims']})
for n in sys.argv[1:]: process(n)
