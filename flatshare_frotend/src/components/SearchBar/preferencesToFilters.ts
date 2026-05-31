import type { TenantPreferences } from "../../models/tenantPreferences";
import type { City, Filters } from "./FiltersStore";
import { locationConfig } from "./locationConfig";

export function resolveLocationFromPreferredDistricts(
  preferredDistricts: string[]
): { city: City | ""; district: string } {
  if (!preferredDistricts.length) {
    return { city: "", district: "" };
  }

  for (const [city, districts] of Object.entries(locationConfig)) {
    for (const preferred of preferredDistricts) {
      const normalized = preferred.trim().toLowerCase();
      if (!normalized) continue;

      const match = districts.find((d) => d.toLowerCase() === normalized);
      if (match) {
        return { city: city as City, district: match };
      }
    }
  }

  return { city: "", district: "" };
}

export function preferencesToFilters(
  prefs: TenantPreferences
): Partial<Filters> {
  const partial: Partial<Filters> = {};

  const maxPrice = Number(prefs.maxPrice);
  if (prefs.maxPrice != null && !Number.isNaN(maxPrice)) {
    partial.maxPrice = String(Math.round(maxPrice));
  }

  if (prefs.petsAllowed === true) {
    partial.petsAllowed = true;
  }

  if (prefs.smokingAllowed === false) {
    partial.nonSmokingOnly = true;
  }

  const { city, district } = resolveLocationFromPreferredDistricts(
    prefs.preferredDistricts ?? []
  );
  if (city) partial.city = city;
  if (district) partial.district = district;

  return partial;
}
