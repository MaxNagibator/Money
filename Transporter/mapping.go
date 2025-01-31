package main

import (
	. "transporter/entities"
)

type TransporterMapping struct {
	AuthUser   AuthUser
	DebtOwner  DebtOwner
	Debt       Debt
	DomainUser DomainUser
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
	}
}
