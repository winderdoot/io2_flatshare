import { useQuery } from "@tanstack/react-query";
import { useFiltersStore } from "../SearchBar/FiltersStore";
import { API_URL } from "../../config";
import { cityOptions } from "../SearchBar/locationConfig";
import type { ListingDTO } from "../../models/listing";
import {getListingThumbnail} from "../../images_service/ImagesService";

export interface Listing {
  id: string;
  title: string;
  price: number;
  city: string;
  district: string;
  area: number;
}

type StoredUser = { id?: string; role?: string } | null;

const PAGE_SIZE = 5;

const readStoredUser = (): StoredUser => {
  try {
    const raw = localStorage.getItem("user");
    if (!raw) return null;
    return JSON.parse(raw) as StoredUser;
  } catch {
    return null;
  }
};

const resolveCity = (rawCity: unknown): string | null => {
  if (typeof rawCity !== "string" || rawCity === "") return null;
  const mapped = cityOptions[rawCity as keyof typeof cityOptions];
  return mapped ?? rawCity;
};

const filterMatchesActiveListing = (
  listing: ListingDTO,
  filters: Record<string, any>
): boolean => {
  if (listing.status !== "Active") return false;

  const district = filters.district;
  if (typeof district === "string" && district !== "") {
    if (listing.location?.district !== district) return false;
  }

  const minPrice = Number(filters.minPrice);
  if (!Number.isNaN(minPrice) && filters.minPrice !== "" && listing.price < minPrice) {
    return false;
  }
  const maxPrice = Number(filters.maxPrice);
  if (!Number.isNaN(maxPrice) && filters.maxPrice !== "" && listing.price > maxPrice) {
    return false;
  }

  const minArea = Number(filters.minArea);
  if (!Number.isNaN(minArea) && filters.minArea !== "" && listing.area < minArea) {
    return false;
  }
  const maxArea = Number(filters.maxArea);
  if (!Number.isNaN(maxArea) && filters.maxArea !== "" && listing.area > maxArea) {
    return false;
  }

  if (filters.petsAllowed === true && listing.attributes?.petsAllowed === false) {
    return false;
  }
  if (filters.nonSmokingOnly === true && listing.attributes?.nonSmokingOnly === false) {
    return false;
  }
  if (filters.closeToShops === true && listing.attributes?.closeToShops === false) {
    return false;
  }

  return true;
};

const buildMatchesQueryParams = (filters: Record<string, any>, page: number) => {
  const params = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value === "" || value === false) return;
    if (key === "city")
      params.append("city", cityOptions[value as keyof typeof cityOptions]);
    else
      params.append(key, String(value));
  });

  params.append("size", String(PAGE_SIZE));
  params.append("page", String(page));

  return params.toString();
};

const wrapAsPage = (filtered: ListingDTO[], page: number) => {
  const totalElements = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalElements / PAGE_SIZE));
  const pageItems = filtered.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE);
  return {
    content: pageItems.map((listing) => ({ listing, matchScore: 0 })),
    page: {
      size: PAGE_SIZE,
      number: page,
      totalElements,
      totalPages,
    },
  };
};

const fetchFromListings = async (filters: Record<string, any>, page: number) => {
  const params = new URLSearchParams();
  const city = resolveCity(filters.city);
  if (city) params.append("city", city);
  if (typeof filters.district === "string" && filters.district !== "") {
    params.append("district", filters.district);
  }

  const url = params.toString()
    ? `${API_URL}/api/v1/listings?${params.toString()}`
    : `${API_URL}/api/v1/listings`;

  const res = await fetch(url, {
    method: "GET",
    headers: { Accept: "application/json" },
  });
  if (!res.ok) {
    throw new Error("Błąd pobierania ogłoszeń");
  }

  const all = (await res.json()) as ListingDTO[];
  const filtered = all.filter((item) => filterMatchesActiveListing(item, filters));
  return wrapAsPage(filtered, page);
};

const fetchFromMatches = async (
  filters: Record<string, any>,
  page: number,
  token: string
) => {
  const query = buildMatchesQueryParams(filters, page);
  const res = await fetch(`${API_URL}/api/v1/matches?${query}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
  });
  if (!res.ok) {
    throw new Error("Błąd pobierania ogłoszeń");
  }

  return await res.json();
};

const fetchListings = async (
  filters: Record<string, any>,
  page: number
): Promise<any> => {
  const user = readStoredUser();
  const token = localStorage.getItem("token");
  const isTenant = user?.role === "TENANT" && !!token;

  if (isTenant) {
    return fetchFromMatches(filters, page, token!);
  }
  return fetchFromListings(filters, page);
};


export const useListings = () => {
  const { appliedFilters, page } = useFiltersStore();

  return useQuery({
    queryKey: ["listings", appliedFilters, page],
    queryFn: () => fetchListings(appliedFilters, page),
    staleTime: 1000 * 30,
    refetchOnWindowFocus: false
  });
};
