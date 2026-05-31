import { useEffect, useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import type { TenantPreferences } from "../../models/tenantPreferences";
import { tenantPreferencesService } from "../../pages/TenantPreferences/TenantPreferencesService";
import { useFiltersStore } from "./FiltersStore";
import { preferencesToFilters } from "./preferencesToFilters";

/** API may return camelCase or PascalCase depending on serializer settings. */
function normalizePreferences(raw: Record<string, unknown>): TenantPreferences {
  const pick = <T>(camel: string, pascal: string): T | null | undefined => {
    if (raw[camel] !== undefined) return raw[camel] as T;
    if (raw[pascal] !== undefined) return raw[pascal] as T;
    return undefined;
  };

  const districts = pick<string[] | null>("preferredDistricts", "PreferredDistricts");

  return {
    maxPrice: pick<number | null>("maxPrice", "MaxPrice") ?? null,
    currency: pick<string | null>("currency", "Currency") ?? null,
    smokingAllowed: pick<boolean | null>("smokingAllowed", "SmokingAllowed") ?? null,
    petsAllowed: pick<boolean | null>("petsAllowed", "PetsAllowed") ?? null,
    preferredDistricts: districts ?? null,
  };
}

/**
 * Loads tenant preferences into search filters when the offer page mounts.
 * Returns true when it is safe to run debounced search (auth known, prefs applied or skipped).
 */
export function useSearchPreferences(): boolean {
  const { user, token, loading: authLoading } = useAuth();
  const initFromPreferences = useFiltersStore((s) => s.initFromPreferences);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (authLoading) {
      setReady(false);
      return;
    }

    if (user?.role !== "TENANT" || !token) {
      setReady(true);
      return;
    }

    let cancelled = false;
    setReady(false);

    (async () => {
      try {
        const raw = await tenantPreferencesService.get(token);
        if (cancelled) return;
        const prefs = normalizePreferences(raw as unknown as Record<string, unknown>);
        initFromPreferences(preferencesToFilters(prefs));
      } catch {
        /* Pre-fill optional; search still works */
      } finally {
        if (!cancelled) setReady(true);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [authLoading, user?.role, token, initFromPreferences]);

  return ready;
}
