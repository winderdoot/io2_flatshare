import type { ListingDTO } from "../../models/listing";
import coverImage from "../../assets/rent_house.png";

/** Mocki dopóki nie ma `GET /api/v1/Listings/{id}` w GUI. */
export const MOCK_LISTINGS: ListingDTO[] = [
  {
    id: "550e8400-e29b-41d4-a716-446655440001",
    title: "Jasny pokój w centrum — współdzielone mieszkanie",
    description:
      "Oferujemy przestronny, słoneczny pokój w trzypokojowym mieszkaniu po remoncie. " +
      "Wspólna kuchnia i łazienka, szybki internet, pralka w mieszkaniu. " +
      "Idealne na semestr lub dłużej — współlokatorzy to spokojni studenci. " +
      "Blisko komunikacji, sklepy i kawiarnie w 5 minutach pieszo.",
    price: 1200,
    currency: "PLN",
    availableSince: "2025-09-01",
    availableUntil: "2026-08-31",
    ownerContact: "+48 123 456 789",
    area: 14.5,
    location: {
      city: "Warszawa",
      district: "Śródmieście",
      street: "Marszałkowska",
      aptNumber: "12",
    },
    attributes: {
      petsAllowed: false,
      nonSmokingOnly: true,
      closeToShops: true,
      profile: "Student",
    },
    coverImageUrl: coverImage,
  },
  {
    id: "550e8400-e29b-41d4-a716-446655440002",
    title: "Kameralny pokój — krótki pobyt",
    description:
      "Mniejszy pokój, świetny na kilka tygodni lub miesięcy. Cicha kamienica, " +
      "drzwi antywłamaniowe, domofon. Świetna baza pod zwiedzanie miasta.",
    price: 95,
    currency: "PLN",
    availableSince: "2025-06-15",
    availableUntil: "2025-09-15",
    ownerContact: "kontakt@example.com",
    area: 10,
    location: {
      city: "Kraków",
      district: "Stare Miasto",
      street: "Grodzka",
      aptNumber: "3A",
    },
    attributes: {
      petsAllowed: true,
      nonSmokingOnly: false,
      closeToShops: true,
      profile: "Tourist",
    },
    coverImageUrl: coverImage,
  },
];

export function findMockListing(id: string): ListingDTO | undefined {
  return MOCK_LISTINGS.find((l) => l.id === id);
}
