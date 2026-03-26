# SDD — LimpioHogar Agenda
**Version:** 2.0
**Fecha:** Marzo 2026
**Tipo:** Progressive Web App (PWA)
**Stack:** Blazor WASM (.NET 9) + Supabase
**URL producción:** app.limpiohogar.cl
**Repositorio:** github.com/rodrigotobarc-ui/limpiohogar-agenda
**Diseño Figma:** https://www.figma.com/make/ObKQ3mtX4ovGjtAmQbEKBg/Mobile-PWA-for-LimpioHogar

---

## 1. Resumen Ejecutivo

LimpioHogar Agenda es la aplicación de gestión interna de LimpioHogar SpA.
Permite administrar trabajadoras, clientes, servicios agendados y pagos/comisiones
desde cualquier dispositivo móvil, sin necesidad de instalarla desde una tienda de apps.

Se accede desde Safari/Chrome en app.limpiohogar.cl y se instala como acceso directo
en la pantalla de inicio del teléfono (PWA).

**Usuarios:**
- Rodrigo Tobar — administrador, acceso total
- Asistente — acceso operacional (sin pagos ni comisiones)

---

## 2. Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Frontend | Blazor WASM (.NET 9) |
| PWA | Blazor PWA template |
| UI | MudBlazor |
| Diseño UI | Figma (via Figma MCP) |
| Backend/DB | Supabase (PostgreSQL + Auth + Realtime) |
| Hosting | Vercel (subdominio app.limpiohogar.cl) |
| Repositorio | GitHub (limpiohogar-agenda) |
| Notificaciones WhatsApp | wa.me links (manual) |
| Notificaciones correo | Supabase Edge Functions + Resend |
| CI/CD | GitHub Actions → Vercel (auto-deploy en push a main) |

---

## 3. Diseño en Figma

El diseño fue creado en Figma Make y aprobado antes del desarrollo.
Contiene 6 pantallas principales diseñadas para iPhone 14 (390x844px).

**Pantallas diseñadas:**
1. Login
2. Dashboard
3. Agenda Semanal
4. Nuevo Servicio
5. Trabajadoras
6. Pagos Mensuales

**Paleta de colores:**
```css
--color-primary:  #1E2D4E;  /* azul marino */
--color-accent:   #4CAF9A;  /* verde menta */
--color-bg:       #FFFFFF;
--color-bg-alt:   #F5F7FA;
```

**Tipografía:** Inter

**Integración Figma MCP:**
El desarrollo usa Figma MCP conectado en Claude Code para leer directamente los
componentes, colores y layouts del diseño sin exportar assets manualmente.
- Instalación: `claude plugin install figma`
- Permite inspeccionar frames y variables de diseño en tiempo real
- Claude Code consulta cada frame de Figma al implementar cada pantalla

---

## 4. Requisitos de Diseño Responsivo

- Diseño base: iPhone 14 (390px)
- Compatible con: iPhone SE (375px) e iPhone Pro Max (430px)
- Todos los campos de formulario: ancho 100%, altura mínima 48px
- Tap targets mínimo 44x44px (Apple HIG)
- Formulario Nuevo Servicio: layout una columna en móvil
- Fecha y Hora en una sola fila dividida 50/50
- Selectores de Plan: cards apiladas verticalmente
- Panel Distribución de Pago: sticky bottom
- Breakpoint 768px+: layout dos columnas permitido en formularios
- Bottom navigation bar fija en todas las pantallas principales

---

## 5. Estructura de Archivos

```
limpiohogar-agenda/
├── LimpioHogar.Agenda.sln
├── src/
│   └── LimpioHogar.Agenda.Client/
│       ├── Pages/
│       │   ├── Auth/
│       │   │   └── Login.razor
│       │   ├── Dashboard/
│       │   │   └── Index.razor
│       │   ├── Trabajadoras/
│       │   │   ├── Index.razor
│       │   │   ├── Detalle.razor
│       │   │   └── Formulario.razor
│       │   ├── Clientes/
│       │   │   ├── Index.razor
│       │   │   ├── Detalle.razor
│       │   │   └── Formulario.razor
│       │   ├── Agenda/
│       │   │   ├── Index.razor
│       │   │   ├── Semana.razor
│       │   │   └── Formulario.razor
│       │   └── Pagos/
│       │       ├── Index.razor
│       │       └── Resumen.razor
│       ├── Services/
│       │   ├── SupabaseService.cs
│       │   ├── TrabajadoraService.cs
│       │   ├── ClienteService.cs
│       │   ├── ServicioService.cs
│       │   └── PagoService.cs
│       ├── Models/
│       │   ├── Trabajadora.cs
│       │   ├── Cliente.cs
│       │   ├── Servicio.cs
│       │   └── Pago.cs
│       ├── wwwroot/
│       │   ├── manifest.json
│       │   ├── service-worker.js
│       │   └── icons/
│       └── Program.cs
```

---

## 6. Modelo de Datos

### 6.1 Tabla: trabajadoras

```sql
CREATE TABLE trabajadoras (
  id                uuid         DEFAULT gen_random_uuid() PRIMARY KEY,
  nombre            varchar(100) NOT NULL,
  apellido          varchar(100) NOT NULL,
  telefono          varchar(20)  NOT NULL,
  email             varchar(100),
  direccion         varchar(255),
  comuna            varchar(100),
  estado_migratorio varchar(50)  DEFAULT 'no_informado',
  -- valores: regular | en_tramite | irregular | no_informado
  activa            boolean      DEFAULT true,
  notas             text,
  created_at        timestamp    DEFAULT now()
);
```

### 6.2 Tabla: clientes

```sql
CREATE TABLE clientes (
  id          uuid         DEFAULT gen_random_uuid() PRIMARY KEY,
  nombre      varchar(100) NOT NULL,
  apellido    varchar(100) NOT NULL,
  telefono    varchar(20)  NOT NULL,
  email       varchar(100),
  direccion   varchar(255) NOT NULL,
  comuna      varchar(100) NOT NULL,
  tipo        varchar(20)  DEFAULT 'casa',
  -- valores: casa | oficina
  notas       text,
  activo      boolean      DEFAULT true,
  created_at  timestamp    DEFAULT now()
);
```

### 6.3 Tabla: servicios

```sql
CREATE TABLE servicios (
  id                uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
  trabajadora_id    uuid        REFERENCES trabajadoras(id) NOT NULL,
  cliente_id        uuid        REFERENCES clientes(id) NOT NULL,
  fecha             date        NOT NULL,
  hora              time        NOT NULL,
  plan              varchar(20) NOT NULL,
  -- valores: basico | premium | deluxe
  estado            varchar(20) DEFAULT 'pendiente',
  -- valores: pendiente | confirmado | realizado | cancelado
  monto_total       integer     NOT NULL,
  monto_trabajadora integer     NOT NULL,
  monto_agencia     integer     NOT NULL,
  notas             text,
  created_at        timestamp   DEFAULT now()
);
```

### 6.4 Tabla: pagos

```sql
CREATE TABLE pagos (
  id          uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
  servicio_id uuid        REFERENCES servicios(id) NOT NULL,
  fecha_pago  date        NOT NULL,
  monto       integer     NOT NULL,
  tipo        varchar(20) NOT NULL,
  -- valores: ingreso_cliente | egreso_trabajadora
  confirmado  boolean     DEFAULT false,
  notas       text,
  created_at  timestamp   DEFAULT now()
);
```

### 6.5 Montos por plan

| Plan | Total cliente | Trabajadora | Agencia |
|---|---|---|---|
| Basico | $35.000 | $25.000 | $10.000 |
| Premium | $55.000 | $40.000 | $15.000 |
| Deluxe | $85.000 | $65.000 | $20.000 |

---

## 7. Módulos de la App

### 7.1 Autenticación
- Login con email y password via Supabase Auth
- Dos roles: admin y asistente
- Sesión persistente en el dispositivo
- Sin registro público — solo admin crea usuarios

### 7.2 Dashboard
- Saludo con nombre y fecha actual
- Lista servicios de hoy (trabajadora, cliente, hora, estado con badge de color)
- Card comisión del mes (solo admin)
- Accesos rápidos: Agendar, Nueva Trabajadora, Nuevo Cliente

### 7.3 Trabajadoras
- Listado con búsqueda por nombre o comuna
- Cards con nombre, teléfono, comuna, toggle activa/inactiva
- Formulario crear/editar
- Detalle con historial de servicios
- Estado migratorio visible solo para admin

### 7.4 Clientes
- Listado con búsqueda por nombre o comuna
- Cards con nombre, teléfono, comuna, tipo (casa/oficina)
- Formulario crear/editar
- Detalle con historial de servicios

### 7.5 Agenda
- Vista semanal con navegación entre semanas
- Servicios con código de color por estado:
  - Amarillo: Pendiente
  - Verde claro: Confirmado
  - Verde oscuro: Realizado
  - Rojo: Cancelado
- Tap en servicio → detalle
- Botón FAB (+) → Nuevo Servicio

**Formulario Nuevo Servicio:**
- Trabajadora (select de activas)
- Cliente (select de activos)
- Fecha (date picker, no permite fechas pasadas)
- Hora (time picker, intervalos 30 min)
- Plan (Basico / Premium / Deluxe)
- Notas (opcional)
- Panel sticky bottom con distribución de pago calculada automáticamente

**Cambio de estado:**
- Pendiente → Confirmado → Realizado
- Cualquier estado → Cancelado

### 7.6 Pagos y Comisiones (solo admin)
- Al marcar servicio como realizado se crean 2 registros automáticos:
  - ingreso_cliente: monto_total
  - egreso_trabajadora: monto_trabajadora
- Toggle para confirmar cada pago
- Resumen mensual: ingresos brutos, pagado trabajadoras, comisión neta agencia
- Desglose por trabajadora

---

## 8. Notificaciones

### 8.1 WhatsApp a trabajadora (al agendar)
```
https://wa.me/569{telefono}?text=Hola+{nombre}%2C+tienes+un+servicio+
agendado+el+{fecha}+a+las+{hora}+en+{direccion}%2C+{comuna}.+
Plan+{plan}.+Confirma+tu+asistencia.+-+LimpioHogar
```

### 8.2 WhatsApp a cliente (al confirmar)
```
https://wa.me/569{telefono}?text=Hola+{nombre}%2C+confirmamos+tu+
servicio+de+aseo+el+{fecha}+a+las+{hora}.+
Trabajadora%3A+{nombre_trabajadora}.+
Valor%3A+%24{monto_total}.+-+LimpioHogar
```

### 8.3 Correo a Rodrigo (al agendar)
Supabase Edge Function → Resend API

```
Asunto: Nuevo servicio agendado - {fecha}
Trabajadora: {nombre_trabajadora}
Cliente: {nombre_cliente} - {direccion}
Fecha: {fecha} {hora}
Plan: {plan} - ${monto_total}
Comision agencia: ${monto_agencia}
```

---

## 9. PWA

### 9.1 manifest.json
```json
{
  "name": "LimpioHogar Agenda",
  "short_name": "LimpioHogar",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#FFFFFF",
  "theme_color": "#1E2D4E",
  "icons": [
    { "src": "icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

### 9.2 Instalacion en iPhone
```
Safari → app.limpiohogar.cl
→ Boton compartir (cuadrado con flecha hacia arriba)
→ Agregar a pantalla de inicio
→ Queda icono LimpioHogar en el telefono
```

### 9.3 Componentes MudBlazor
- MudDataGrid → listados
- MudDatePicker → selector fecha
- MudTimePicker → selector hora
- MudSelect → desplegables
- MudSnackbar → notificaciones inline
- MudBottomNavigation → navegacion mobile

### 9.4 Navegacion mobile
Bottom navigation bar fija con 4 secciones:
- Inicio (Dashboard)
- Agenda
- Personas (Trabajadoras y Clientes)
- Pagos (solo visible para admin)

---

## 10. Roles y Permisos

| Modulo | Admin | Asistente |
|---|---|---|
| Dashboard | Completo | Sin comisiones |
| Trabajadoras | Todo + estado migratorio | Sin estado migratorio |
| Clientes | Todo | Todo |
| Agenda | Todo | Todo |
| Pagos | Todo | Sin acceso |
| Comisiones | Todo | Sin acceso |
| Crear usuarios | Si | No |

---

## 11. Plan de Desarrollo

| # | Tarea |
|---|---|
| 1 | Crear proyecto Supabase agenda (separado del de landing) |
| 2 | Crear tablas y RLS en Supabase |
| 3 | Crear repo GitHub limpiohogar-agenda |
| 4 | Scaffold Blazor WASM PWA con MudBlazor |
| 5 | Configurar Figma MCP en Claude Code |
| 6 | Implementar Auth (login + roles) |
| 7 | CRUD Trabajadoras |
| 8 | CRUD Clientes |
| 9 | Agenda semanal + formulario nuevo servicio |
| 10 | Notificaciones WhatsApp + correo Resend |
| 11 | Pagos y confirmacion |
| 12 | Resumen mensual comisiones |
| 13 | PWA manifest + service worker + iconos |
| 14 | Deploy en app.limpiohogar.cl via Vercel |

---

## 12. Estado de Pendientes Pre-Desarrollo

- [x] LimpioHogar SpA constituida (RUT 78.385.616-6)
- [x] Inicio de actividades SII completado
- [x] Dominio limpiohogar.cl registrado
- [x] Numero WhatsApp Business activo (+56942592444 eSIM Entel)
- [x] Diseno Figma completado y aprobado
- [ ] DNS limpiohogar.cl propagado
- [ ] Subdominio app.limpiohogar.cl configurado en Vercel
- [ ] Proyecto Supabase agenda creado
- [ ] Cuenta Resend para correos transaccionales
- [ ] Figma MCP configurado en Claude Code

---

*LimpioHogar SpA | Santiago, Chile | Marzo 2026*
