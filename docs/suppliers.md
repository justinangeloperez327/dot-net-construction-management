# Suppliers

Group 13 introduces the company-wide supplier master used by procurement.

## Scope

A supplier records:

- supplier code
- name
- category / trade
- contact person
- email
- phone
- address
- registration number
- tax registration number
- Active / Inactive state

The supplier code is the stable internal procurement identifier and is unique across the company.

Supplier names are deliberately **not** unique. Different legal entities can have similar or identical trading names.

## Company-wide master

Suppliers are not owned by a Project.

```text
Supplier
  ↑
  ├── future Purchase Order
  ├── future quotation / comparison
  └── future delivery
```

This avoids duplicating the same supplier for every project.

## Lifecycle

```text
Active ↔ Inactive
```

Inactive suppliers stay readable and can be reactivated. They are not deleted because later procurement records must retain historical supplier references.

The application exposes an active-supplier query for future workflows that need selectable vendors.

## Flexible category

Category / trade remains free text in Group 13.

Examples may include:

```text
MEP
Joinery
Civil Works
Steel
Electrical
HVAC
General Trading
```

A governed supplier-category taxonomy should only be introduced when there is a real approved classification scheme.

## Deliberately deferred

Group 13 does not add:

- supplier qualification / prequalification
- supplier ratings
- blacklist / suspension workflow
- quotation history
- price lists
- bank details
- payment terms
- supplier documents / expiry tracking
- project-specific approved vendor lists

Those concepts require explicit procurement/commercial rules and should not be guessed into the supplier master.

## Permissions

```text
suppliers.view
suppliers.create
suppliers.update
suppliers.set_active
```

## Routes

```text
/suppliers
/suppliers/create
/suppliers/{supplierId}
```

## Persistence

Table:

```text
Suppliers
```

Important constraints:

- primary key on `Id`
- unique index on `SupplierCode`
- index on `Name`
