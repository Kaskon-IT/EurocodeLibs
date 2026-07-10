using System.Globalization;

namespace Eurocode.BetonConstructies
{
    /// <summary>
    /// Genereert een 3D-weergave (Three.js) van de J3-console, als tegenhanger van <see cref="J3ConsoleSvg"/>.
    /// Net als <see cref="J3ConsoleSvg.CreateSvg"/> retourneert deze klasse een kant-en-klare string: een
    /// zelfstandig HTML-document dat Three.js via een CDN import-map laadt. Het is direct herbruikbaar,
    /// bijvoorbeeld in een Blazor-component via <c>&lt;iframe srcdoc="@html"&gt;</c>, zonder JS-interop of build-stappen.
    /// </summary>
    //[Obsolete ("gebruik razor")]
//    public static class J3ConsoleThree
//    {
//        /// <summary>
//        /// Bouwt een volledig, zelfstandig HTML-document met een interactieve 3D-weergave (Three.js)
//        /// van het console-model: kolom, console/nok, oplegplaat, hoofdtrekwapening (met buigstraal),
//        /// het staaf-en-knoopmodel en de krachten.
//        /// </summary>
//        public static string CreateHtml(J3ConsoleResult r, J3ConsoleInput i)
//        {
//            var ci = CultureInfo.InvariantCulture;
//            string N(double v) => v.ToString("0.###", ci);

//            // Zelfde geometrie als in J3ConsoleSvg (model: x naar rechts, y naar beneden)
//            double H = i.H;
//            double L = i.L;
//            double Bw = i.Bw;
//            double B = i.B;

//            double x1 = r.X1;
//            double ac = i.Ac;
//            double c = i.Dekking;
//            double phi = i.HoofdstaafDiameter;

//            double D = r.D;
//            double Z = r.Z;
//            double Z0 = r.Z0;

//            double y2 = H - D;
//            double y1 = y2 + Z;
//            double y3 = (y1 + y2) / 2.0;

//            double dikteOpleg = r.DikteOplegmateriaal;
//            double fX = i.FactorHEd;
//            double dX = i.FactorHEd * (r.H - r.D);

//            double n0x = ac, n0y = 0;
//            double n1x = -x1 / 2.0, n1y = y1;
//            double n2x = ac + dX, n2y = y2;
//            double n3x = n1x, n3y = y3;
//            double n4x = n2x, n4y = y3;

//            double plaatB = i.LoadPlateLength;
//            double plaatX = ac - plaatB / 2.0;
//            double plaatW = i.LoadPlateWidth;

//            // Minimale buigroldiameter (Tabel 8.1N) -> buigstraal hoofdwapening
//            double bendR = (phi <= 16.0 ? 4.0 : 7.0) * phi / 2.0;

//            double fEd = i.FEd;
//            double fc = r.Fc;
//            double ft = r.Ft;
//            double hEd = r.FhEd;

//            return $$$"""
//<!DOCTYPE html>
//<html lang="nl">
//<head>
//<meta charset="utf-8" />
//<meta name="viewport" content="width=device-width, initial-scale=1" />
//<title>J3 Console 3D</title>
//<style>
//  html, body { margin: 0; height: 100%; overflow: hidden; background: #f5f5f5; font-family: Arial, sans-serif; }
//  #app { width: 100vw; height: 100vh; display: block; }
//  #legend {
//    position: fixed; top: 10px; left: 10px; z-index: 10;
//    background: rgba(255,255,255,0.88); padding: 8px 12px; border-radius: 6px;
//    font-size: 12px; color: #333; line-height: 1.5; box-shadow: 0 1px 4px rgba(0,0,0,0.2);
//  }
//  #legend b { color: #000; }
//  #legend .sw { display: inline-block; width: 12px; height: 12px; margin-right: 6px; vertical-align: middle; border-radius: 2px; }
//</style>
//<script type="importmap">
//{
//  "imports": {
//    "three": "https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js",
//    "three/addons/": "https://cdn.jsdelivr.net/npm/three@0.160.0/examples/jsm/"
//  }
//}
//</script>
//</head>
//<body>
//<div id="legend">
//  <div><b>J3 &ndash; korte console</b></div>
//  <div><span class="sw" style="background:#b8b8b8"></span>beton (kolom / console)</div>
//  <div><span class="sw" style="background:#8a5cd6"></span>oplegplaat</div>
//  <div><span class="sw" style="background:#1565c0"></span>hoofdtrekwapening / trekband</div>
//  <div><span class="sw" style="background:#d32f2f"></span>drukdiagonaal</div>
//  <div><span class="sw" style="background:#2e7d32"></span>reactiekracht</div>
//  <div style="margin-top:4px;color:#666">sleep = draaien &middot; scroll = zoom</div>
//</div>
//<div id="app"></div>

//<script type="module">
//import * as THREE from 'three';
//import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

//// ---- rekenwaarden uit het C#-model ----
//const H = {{{N(H)}}}, L = {{{N(L)}}}, Bw = {{{N(Bw)}}}, B = {{{N(B)}}};
//const x1 = {{{N(x1)}}}, ac = {{{N(ac)}}}, c = {{{N(c)}}}, phi = {{{N(phi)}}};
//const bendR = {{{N(bendR)}}};
//const y1 = {{{N(y1)}}}, y2 = {{{N(y2)}}}, y3 = {{{N(y3)}}};
//const dikteOpleg = {{{N(dikteOpleg)}}};
//const fX = {{{N(fX)}}};
//const plaatB = {{{N(plaatB)}}}, plaatX = {{{N(plaatX)}}}, plaatW = {{{N(plaatW)}}};
//const n0 = { x: {{{N(n0x)}}}, y: {{{N(n0y)}}} };
//const n1 = { x: {{{N(n1x)}}}, y: {{{N(n1y)}}} };
//const n2 = { x: {{{N(n2x)}}}, y: {{{N(n2y)}}} };
//const n3 = { x: {{{N(n3x)}}}, y: {{{N(n3y)}}} };
//const n4 = { x: {{{N(n4x)}}}, y: {{{N(n4y)}}} };
//const FEd = {{{N(fEd)}}}, Fc = {{{N(fc)}}}, Ft = {{{N(ft)}}}, HEd = {{{N(hEd)}}};

//// ---- model (y naar beneden) -> Three.js (y omhoog) ----
//const v = (x, ym, z) => new THREE.Vector3(x, -ym, z);

//const group = new THREE.Group();

//const matConcrete = new THREE.MeshStandardMaterial({ color: 0xb8b8b8, transparent: true, opacity: 0.35, roughness: 0.9, metalness: 0.0, side: THREE.DoubleSide });
//const matPlate = new THREE.MeshStandardMaterial({ color: 0x8a5cd6, transparent: true, opacity: 0.65, roughness: 0.6 });
//const matRebar = new THREE.MeshStandardMaterial({ color: 0x1565c0, roughness: 0.5, metalness: 0.3 });
//const matNode = new THREE.MeshStandardMaterial({ color: 0x222222 });
//const matStrut = new THREE.LineDashedMaterial({ color: 0xd32f2f, dashSize: 14, gapSize: 9 });
//const matTie = new THREE.LineBasicMaterial({ color: 0x1565c0 });

//function addEdges(mesh, color) {
//  const seg = new THREE.LineSegments(new THREE.EdgesGeometry(mesh.geometry), new THREE.LineBasicMaterial({ color: color }));
//  mesh.add(seg);
//}

//// ---- massieve delen ----
//// kolom: x in [-Bw, 0], iets doorlopend boven/onder, diepte B
//{
//  const top = -100, bot = H + 200;
//  const geo = new THREE.BoxGeometry(Bw, bot - top, B);
//  const m = new THREE.Mesh(geo, matConcrete);
//  m.position.set(-Bw / 2, -((top + bot) / 2), 0);
//  addEdges(m, 0x777777);
//  group.add(m);
//}

//// console/nok: geextrudeerd profiel over diepte B
//{
//  const shape = new THREE.Shape();
//  shape.moveTo(0, 0);
//  shape.lineTo(L - c, 0);
//  shape.lineTo(L, -c);
//  shape.lineTo(L, -H / 2);
//  shape.lineTo(0, -H);
//  shape.closePath();
//  const geo = new THREE.ExtrudeGeometry(shape, { depth: B, bevelEnabled: false });
//  const m = new THREE.Mesh(geo, matConcrete);
//  m.position.set(0, 0, -B / 2);
//  addEdges(m, 0x777777);
//  group.add(m);
//}

//// oplegplaat
//{
//  const geo = new THREE.BoxGeometry(plaatB, dikteOpleg, plaatW);
//  const m = new THREE.Mesh(geo, matPlate);
//  m.position.set(plaatX + plaatB / 2, dikteOpleg / 2, 0);
//  addEdges(m, 0x5a3a99);
//  group.add(m);
//}

//// ---- hoofdtrekwapening met haken (buigstraal bendR) ----
//function rebar(zOff) {
//  const pts = [
//    v(L - c, y2 + 10 * phi, zOff),   // haak omlaag aan de buitenzijde
//    v(L - c, y2, zOff),
//    v(-Bw + c, y2, zOff),
//    v(-Bw + c, y2 + 14 * phi, zOff)  // verankering omlaag in de kolom
//  ];
//  const curve = new THREE.CatmullRomCurve3(pts, false, 'catmullrom', 0.0);
//  const geo = new THREE.TubeGeometry(curve, 80, phi / 2, 12, false);
//  return new THREE.Mesh(geo, matRebar);
//}
//const halfClear = Math.max(B / 2 - c - phi / 2, 0);
//const zBars = halfClear > 5 ? [-halfClear, 0, halfClear] : [0];
//zBars.forEach(z => group.add(rebar(z)));

//// ---- knopen ----
//function node(nx, nym) {
//  const s = new THREE.Mesh(new THREE.SphereGeometry(Math.max(phi * 0.6, 8), 20, 20), matNode);
//  s.position.copy(v(nx, nym, 0));
//  group.add(s);
//}
//[n0, n1, n2, n3, n4].forEach(n => node(n.x, n.y));

//// ---- staven (druk = rood gestreept, trek = blauw) ----
//function line3(a, b, mat) {
//  const ln = new THREE.Line(new THREE.BufferGeometry().setFromPoints([a, b]), mat);
//  if (mat.isLineDashedMaterial) ln.computeLineDistances();
//  return ln;
//}
//const struts = [
//  [n0, n2], [n1, n2], [n3, n2], [n4, n2], [n1, n3], [n1, n4]
//];
//struts.forEach(([a, b]) => group.add(line3(v(a.x, a.y, 0), v(b.x, b.y, 0), matStrut)));
//// horizontale trekband ter hoogte van knoop 2
//group.add(line3(v(n1.x, y2, 0), v(n2.x, y2, 0), matTie));

//// ---- labels als sprites ----
//function label(text, pos, color) {
//  const cv = document.createElement('canvas');
//  cv.width = 256; cv.height = 128;
//  const ctx = cv.getContext('2d');
//  ctx.fillStyle = color || '#000';
//  ctx.font = 'bold 64px Arial';
//  ctx.textAlign = 'center';
//  ctx.textBaseline = 'middle';
//  ctx.fillText(text, 128, 64);
//  const sp = new THREE.Sprite(new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(cv), transparent: true, depthTest: false }));
//  sp.position.copy(pos);
//  sp.scale.set(90, 45, 1);
//  group.add(sp);
//}
//label('1', v(n1.x - 28, n1.y, 0), '#000');
//label('2', v(n2.x + 28, n2.y, 0), '#000');
//label('3', v(n3.x - 28, n3.y, 0), '#000');
//label('4', v(n4.x + 28, n4.y, 0), '#000');

//// ---- krachten ----
//function arrow(dir, origin, len, color) {
//  group.add(new THREE.ArrowHelper(dir.clone().normalize(), origin, len, color, len * 0.22, len * 0.12));
//}
//const fScale = 180 / Math.max(FEd, 1);
//const feLen = Math.max(FEd * fScale, 60);
//const fhLen = Math.max(HEd * fScale, 40);
//const ftLen = Math.max(Ft * fScale, 60);

//arrow(new THREE.Vector3(0, -1, 0), v(ac, -dikteOpleg - feLen, 0), feLen, 0x111111);   // FEd omlaag op plaat
//arrow(new THREE.Vector3(1, 0, 0), v(ac, -dikteOpleg, 0), fhLen, 0x111111);            // HEd horizontaal

//arrow(new THREE.Vector3(fX, -1, 0), v(ac, 0 - feLen, 0), feLen, 0x111111);   // Ntotaal op knoop 0


//arrow(new THREE.Vector3(0, 1, 0), v(n1.x, H + feLen, 0), feLen, 0x2e7d32);            // reactie omhoog
//arrow(new THREE.Vector3(-1, 0, 0), v(-x1, y2, 0), ftLen, 0x1565c0);                   // Ft trekband

//label('FEd', v(ac, -dikteOpleg - feLen - 32, 0), '#111');
//label('HEd', v(ac + fhLen + 34, -dikteOpleg, 0), '#111');
//label('F', v(n1.x, H + feLen + 32, 0), '#2e7d32');
//label('Ft', v(-x1 - ftLen - 34, y2, 0), '#1565c0');

//// ---- scene ----
//const scene = new THREE.Scene();
//scene.background = new THREE.Color(0xf5f5f5);
//scene.add(group);

//scene.add(new THREE.HemisphereLight(0xffffff, 0x555555, 1.1));
//const dir = new THREE.DirectionalLight(0xffffff, 0.9);
//dir.position.set(1, 2, 2);
//scene.add(dir);

//const bbox = new THREE.Box3().setFromObject(group);
//const center = bbox.getCenter(new THREE.Vector3());
//const size = bbox.getSize(new THREE.Vector3());
//const maxDim = Math.max(size.x, size.y, size.z, 1);

//const app = document.getElementById('app');
//const camera = new THREE.PerspectiveCamera(45, app.clientWidth / app.clientHeight, 1, maxDim * 50);
//camera.position.set(center.x + maxDim * 0.9, center.y + maxDim * 0.6, center.z + maxDim * 1.5);

//const renderer = new THREE.WebGLRenderer({ antialias: true });
//renderer.setPixelRatio(window.devicePixelRatio);
//renderer.setSize(app.clientWidth, app.clientHeight);
//app.appendChild(renderer.domElement);

//const controls = new OrbitControls(camera, renderer.domElement);
//controls.target.copy(center);
//controls.enableDamping = true;
//controls.update();

//function onResize() {
//  camera.aspect = app.clientWidth / app.clientHeight;
//  camera.updateProjectionMatrix();
//  renderer.setSize(app.clientWidth, app.clientHeight);
//}
//window.addEventListener('resize', onResize);

//function animate() {
//  requestAnimationFrame(animate);
//  controls.update();
//  renderer.render(scene, camera);
//}
//animate();
//</script>
//</body>
//</html>
//""";
//        }
//    }
}
