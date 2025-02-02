package main

import (
	. "transporter/entities"
)

type TransporterMapping struct {
	AuthUser      AuthUser
	DebtOwner     DebtOwner
	Debt          Debt
	DomainUser    DomainUser
	Category      Category
	Operation     Operation
	FastOperation FastOperation
}

type TableMapping[O any, N any] interface {
	GetBaseTable() *BaseTable[O, N]
	Transform(old O) N
}

func CreateTransporter() *TransporterMapping {
	return &TransporterMapping{
		AuthUser: AuthUser{
			BaseTable: BaseTable[OldAuthUser, NewAuthUser]{
				OldName: `"System"."User"`,
				NewName: `"AspNetUsers"`,
			},
		},
		DomainUser: DomainUser{
			BaseTable: BaseTable[OldDomainUser, NewDomainUser]{
				OldName: `"System"."User"`,
				NewName: "domain_users",
			},
		},
		DebtOwner: DebtOwner{
			BaseTable: BaseTable[OldDebtOwner, NewDebtOwner]{
				OldName: `"Money"."DebtUser"`,
				NewName: "debt_owners",
			},
		},
		Debt: Debt{
			BaseTable: BaseTable[OldDebt, NewDebt]{
				OldName: `"Money"."Debt"`,
				NewName: "debts",
			},
		},
		Category: Category{
			BaseTable: BaseTable[OldCategory, NewCategory]{
				OldName: `"Money"."Category"`,
				NewName: "categories",
			},
		},
		Operation: Operation{
			BaseTable: BaseTable[OldOperation, NewOperation]{
				OldName: `"Money"."Payment"`,
				NewName: "operations",
			},
		},
		FastOperation: FastOperation{
			BaseTable: BaseTable[OldFastOperation, NewFastOperation]{
				OldName: `"Money"."FastOperation"`,
				NewName: "fast_operations",
			},
		},
	}
}
