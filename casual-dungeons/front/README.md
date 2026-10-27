# Casual Dungeons — Frontend (Angular 22)

Frontend en Angular 22 (Signals, standalone, zoneless) para el gestor de campañas de D&D 5e.

## Stack

- **Angular 22**: `provideZonelessChangeDetection()`, componentes standalone, sin NgModules.
- **Signals** para todo el estado (Auth, Theme, Realtime) en vez de `BehaviorSubject`/RxJS clásico.
- **`httpResource`** para peticiones GET reactivas (dashboard, listados); `HttpClient` directo para mutaciones (POST/PUT/DELETE), siguiendo la recomendación oficial de no usar `httpResource` para escritura.
- **Signal Forms** (`@angular/forms/signals`) únicamente en el login. El resto de formularios usan Signals simples + validación manual, por decisión explícita: para los flujos con autoguardado (ficha de personaje) se prefirió un patrón de debounce manual y predecible, dado que Signal Forms es una API muy reciente.
- **SignalR** (`@microsoft/signalr`) para sincronización en tiempo real entre el DM y los jugadores.
- **Drag & drop nativo** (HTML5 Drag and Drop API) para reordenar la iniciativa en combate, sin dependencia de Angular CDK.

## Instalación

```bash
npm install
npm start
```

La app espera el backend en `https://localhost:5001` (configurable en `src/environments/environment.ts`).

## ⚠️ Nota importante sobre Signal Forms

El `LoginComponent` usa la API de **Signal Forms**, estabilizada muy recientemente en Angular 22. Al ser tan nueva, verifica la sintaxis exacta (`[field]`, `form()`, `submit()`, validadores) contra la versión específica de `@angular/forms` que termines instalando — es posible que haya pequeños ajustes de nomenclatura entre la versión consultada al escribir este código y la que tengas en `package.json`. Si algo no compila, la guía oficial está en `angular.dev/guide/forms/signals`.

## ⚠️ Posible conflicto de peer dependencies al instalar (TypeScript)

`@angular/build` actualiza con frecuencia su rango de peer dependency aceptado para `typescript`, y a veces ese rango es más estricto que el que `package.json` declara aquí. Si `npm install` falla con un error `ERESOLVE` mencionando `typescript`:

```bash
npm install --legacy-peer-deps
```

Esto no es un error del proyecto: es un desfase de tooling entre versiones de `@angular/build` publicadas en distintos momentos. Este `package.json` deja `typescript` con un rango abierto (`>=5.9.0`, sin tope superior) precisamente para minimizar el riesgo, pero si `@angular/build` se actualiza a una versión que exige una mayor de TypeScript que la que tengas instalada globalmente, usa el flag anterior o instala la versión de TypeScript que el mensaje de error indique explícitamente.

## Estructura

```
src/app/
  core/           Modelos, servicios HTTP, guards, interceptors (Auth, Theme, Realtime)
  shared/         Componentes reutilizables (TopBar, CampaignCard, ConfirmDialog, StatBox, CornerFrame)
  features/
    auth/login
    dashboard
    admin/user-list, admin/user-form
    campaigns/campaign-detail, campaigns/campaign-form
    character-sheet/         Ficha completa con 7 pestañas
      tabs/general-tab
      tabs/stats-tab
      tabs/combat-tab
      tabs/inventory-tab
      tabs/skills-tab
      tabs/spells-tab
      tabs/notes-tab
    dm-view/players-tab, dm-view/npc-tab (+npc-form), dm-view/combat-tab
    settings
```

## Sistema de diseño — "Casual Dungeons"

Tema de manuscrito iluminado medieval, modo oscuro por defecto (requisito de producto). Tokens en `src/styles/_tokens.scss`:

- **Tipografía**: Cinzel (títulos), Crimson Text (cuerpo), JetBrains Mono (cifras de ficha: PV, CA, atributos).
- **Paleta oscura**: cuero envejecido (`#1a1410`) + dorado de iluminación (`#c9a227`).
- **Paleta clara**: pergamino real (`#f4ecd8`).
- **Elemento de firma**: `CornerFrameComponent`, una orla de esquinas decorativas dibujada en SVG puro (sin imágenes externas), que enmarca tarjetas de campaña y NPC.

## Autoguardado de la ficha de personaje

`CharacterSheetComponent` centraliza el autoguardado de las pestañas General/Estadísticas/Combate:
- Cada cambio actualiza el estado local de inmediato (incluyendo un **recálculo optimista local** de modificadores de atributo y bonificador de competencia, replicando las fórmulas D&D 5e del backend, para que la UI no muestre cifras desactualizadas mientras espera la confirmación del servidor).
- Un debounce de 800ms agrupa cambios rápidos en una sola petición PUT.
- Al destruir el componente (cambiar de pestaña/navegar fuera), se fuerza un guardado inmediato de cualquier cambio pendiente.

Las pestañas Inventario, Habilidades, Hechizos y Notas no usan este debounce: cada ítem es una entidad independiente con su propio endpoint, así que se guardan al instante (alta/baja) o al perder el foco del campo (edición de notas).

## Correcciones aplicadas al backend durante la integración

Al construir el frontend se detectaron dos huecos reales en la API que fue necesario corregir en el backend (incluidos en el zip del backend entregado):

1. **`GET /api/usuarios/jugadores`**: no existía ninguna forma de que un DM listara jugadores para invitarlos a una campaña (el único endpoint de listado de usuarios estaba restringido a Administrador). Se añadió un endpoint acotado que expone solo Id + nombre de usuario.
2. **`GET /api/hechizos`**: la tabla de catálogo de hechizos existía en el dominio y la base de datos, pero no había ningún controlador para consultarla, por lo que un jugador no tenía forma de saber qué hechizos existían para añadir a su personaje. Se añadió `HechizosController` de solo lectura.

## Pendiente / fuera de alcance de este entregable

- El catálogo de hechizos (`dnd.Hechizos`) se crea vacío por el script SQL; necesitarás poblarlo con datos (manualmente o vía script de carga) para que la pestaña de Hechizos sea útil en campañas con lanzadores de conjuros.
- No se generó un favicon personalizado; `index.html` referencia `favicon.ico` en `public/`, que deberás añadir.
- No se ejecutó `ng build`/`npm install` real en este entorno (sin Node.js disponible), por lo que, aunque se verificó exhaustivamente la consistencia de tipos, selectores, inputs/outputs y nombres de métodos de forma estática, se recomienda correr `npm install && ng build` como primer paso para detectar cualquier error de compilación no capturado por la revisión manual.
