export type TenantPreferences = {
  maxPrice: number | null;
  currency: string | null;
  smokingAllowed: boolean | null;
  petsAllowed: boolean | null;
  preferredDistricts: string[] | null;
};
